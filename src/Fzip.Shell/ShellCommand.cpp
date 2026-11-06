#define UNICODE
#define _UNICODE
#include <windows.h>
#include <shobjidl.h>
#include <shlwapi.h>
#include <string>
#include <vector>
#include <new>
#include "CommandLine.h"

// Keep the CLSID identical to packaging/shell/AppxManifest.xml.
const CLSID CLSID_FzipCommand = {0xb09d5c80, 0x77d3, 0x4faf, {0xa4, 0xb7, 0x12, 0x8f, 0xe5, 0xae, 0x90, 0x61}};
static HMODULE moduleHandle;
static LONG objects = 0;
static LONG locks = 0;

static std::wstring AppDirectory() {
    std::vector<wchar_t> path(32768);
    DWORD length = GetModuleFileNameW(moduleHandle, path.data(), static_cast<DWORD>(path.size()));
    if (!length || length >= path.size()) return {};
    std::wstring result(path.data(), length);
    auto slash = result.find_last_of(L"\\/");
    return slash == std::wstring::npos ? L"" : result.substr(0, slash);
}

static bool IsArchive(const std::wstring& path) {
    const wchar_t* extension = PathFindExtensionW(path.c_str());
    const wchar_t* supported[] = {L".zip", L".7z", L".rar", L".arj", L".alz", L".egg", L".cab", L".lzh", L".tar", L".gz", L".bz2", L".xz", L".001"};
    for (auto candidate : supported) if (_wcsicmp(extension, candidate) == 0) return true;
    return false;
}

static HRESULT ReadSelection(IShellItemArray* items, std::vector<std::wstring>& paths, bool& extract) {
    extract = false;
    if (!items) return S_OK; // Folder/desktop background: open the create dialog.
    DWORD count = 0;
    HRESULT hr = items->GetCount(&count);
    if (FAILED(hr)) return hr;
    if (count > 128) return HRESULT_FROM_WIN32(ERROR_TOO_MANY_NAMES);
    size_t length = 0;
    for (DWORD i = 0; i < count; ++i) {
        IShellItem* item = nullptr;
        hr = items->GetItemAt(i, &item);
        if (FAILED(hr)) return hr;
        SFGAOF attributes = 0;
        hr = item->GetAttributes(SFGAO_FILESYSTEM | SFGAO_FOLDER | SFGAO_STREAM, &attributes);
        if (FAILED(hr) || !(attributes & SFGAO_FILESYSTEM)) { item->Release(); return E_INVALIDARG; }
        PWSTR path = nullptr;
        hr = item->GetDisplayName(SIGDN_FILESYSPATH, &path);
        item->Release();
        if (FAILED(hr)) return hr;
        try { paths.emplace_back(path); }
        catch (...) { CoTaskMemFree(path); throw; }
        CoTaskMemFree(path);
        length += paths.back().size() * 2 + 3;
        if (length > 28000) return HRESULT_FROM_WIN32(ERROR_FILENAME_EXCED_RANGE);
        // Explorer also marks ZIP/RAR as browsable folders; streams remain files.
        if (count == 1 && (!(attributes & SFGAO_FOLDER) || (attributes & SFGAO_STREAM))) extract = IsArchive(paths.back());
    }
    return S_OK;
}

class Command final : public IExplorerCommand {
    LONG references = 1;
public:
    Command() { InterlockedIncrement(&objects); }
    ~Command() { InterlockedDecrement(&objects); }
    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void** out) override {
        if (!out) return E_POINTER;
        *out = nullptr;
        if (iid == IID_IUnknown || iid == IID_IExplorerCommand) *out = static_cast<IExplorerCommand*>(this);
        else return E_NOINTERFACE;
        AddRef(); return S_OK;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&references); }
    ULONG STDMETHODCALLTYPE Release() override { auto n = InterlockedDecrement(&references); if (!n) delete this; return n; }
    HRESULT STDMETHODCALLTYPE GetTitle(IShellItemArray* items, PWSTR* title) override {
        if (!title) return E_POINTER;
        *title = nullptr;
        try {
            std::vector<std::wstring> paths; bool extract;
            HRESULT hr = ReadSelection(items, paths, extract);
            if (FAILED(hr)) return hr;
            return SHStrDupW(extract ? L"Fzip으로 압축풀기" : L"Fzip으로 압축하기", title);
        } catch (...) { return E_OUTOFMEMORY; }
    }
    HRESULT STDMETHODCALLTYPE GetIcon(IShellItemArray*, PWSTR* icon) override {
        if (!icon) return E_POINTER;
        *icon = nullptr;
        try { return SHStrDupW((L"\"" + AppDirectory() + L"\\assets\\archive.ico\",0").c_str(), icon); }
        catch (...) { return E_OUTOFMEMORY; }
    }
    HRESULT STDMETHODCALLTYPE GetToolTip(IShellItemArray*, PWSTR* text) override { if (!text) return E_POINTER; *text = nullptr; return E_NOTIMPL; }
    HRESULT STDMETHODCALLTYPE GetCanonicalName(GUID* name) override { if (!name) return E_POINTER; *name = CLSID_FzipCommand; return S_OK; }
    HRESULT STDMETHODCALLTYPE GetState(IShellItemArray* items, BOOL, EXPCMDSTATE* state) override {
        if (!state) return E_POINTER;
        *state = ECS_HIDDEN;
        try {
            std::vector<std::wstring> paths; bool extract;
            if (SUCCEEDED(ReadSelection(items, paths, extract))) *state = ECS_ENABLED;
            return S_OK;
        } catch (...) { return E_OUTOFMEMORY; }
    }
    HRESULT STDMETHODCALLTYPE Invoke(IShellItemArray* items, IBindCtx*) override {
        try {
            std::vector<std::wstring> paths; bool extract;
            HRESULT hr = ReadSelection(items, paths, extract);
            if (FAILED(hr)) return hr;
            auto directory = AppDirectory();
            if (directory.empty()) return E_FAIL;
            auto exe = directory + L"\\Fzip.exe";
            auto command = QuoteArgument(exe) + (extract ? L" --extract" : L" --compress");
            for (const auto& path : paths) command += L" " + QuoteArgument(path);
            if (command.size() >= 32767) return HRESULT_FROM_WIN32(ERROR_FILENAME_EXCED_RANGE);
            STARTUPINFOW startup = {}; startup.cb = sizeof(startup);
            PROCESS_INFORMATION process = {};
            // Explicit executable, no shell interpolation, no inherited handles.
            if (!CreateProcessW(exe.c_str(), command.data(), nullptr, nullptr, FALSE, 0, nullptr, directory.c_str(), &startup, &process))
                return HRESULT_FROM_WIN32(GetLastError());
            CloseHandle(process.hThread); CloseHandle(process.hProcess);
            return S_OK;
        } catch (...) { return E_OUTOFMEMORY; }
    }
    HRESULT STDMETHODCALLTYPE GetFlags(EXPCMDFLAGS* flags) override { if (!flags) return E_POINTER; *flags = ECF_DEFAULT; return S_OK; }
    HRESULT STDMETHODCALLTYPE EnumSubCommands(IEnumExplorerCommand** commands) override { if (!commands) return E_POINTER; *commands = nullptr; return E_NOTIMPL; }
};

class Factory final : public IClassFactory {
    LONG references = 1;
public:
    Factory() { InterlockedIncrement(&objects); }
    ~Factory() { InterlockedDecrement(&objects); }
    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void** out) override {
        if (!out) return E_POINTER;
        *out = nullptr;
        if (iid != IID_IUnknown && iid != IID_IClassFactory) return E_NOINTERFACE;
        *out = static_cast<IClassFactory*>(this); AddRef(); return S_OK;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&references); }
    ULONG STDMETHODCALLTYPE Release() override { auto n = InterlockedDecrement(&references); if (!n) delete this; return n; }
    HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown* outer, REFIID iid, void** out) override {
        if (!out) return E_POINTER;
        *out = nullptr;
        if (outer) return CLASS_E_NOAGGREGATION;
        auto command = new(std::nothrow) Command();
        if (!command) return E_OUTOFMEMORY;
        HRESULT hr = command->QueryInterface(iid, out); command->Release(); return hr;
    }
    HRESULT STDMETHODCALLTYPE LockServer(BOOL lock) override { if (lock) InterlockedIncrement(&locks); else InterlockedDecrement(&locks); return S_OK; }
};

extern "C" HRESULT WINAPI DllGetClassObject(REFCLSID clsid, REFIID iid, void** out) {
    if (!out) return E_POINTER;
    *out = nullptr;
    if (clsid != CLSID_FzipCommand) return CLASS_E_CLASSNOTAVAILABLE;
    auto factory = new(std::nothrow) Factory();
    if (!factory) return E_OUTOFMEMORY;
    HRESULT hr = factory->QueryInterface(iid, out); factory->Release(); return hr;
}
extern "C" HRESULT WINAPI DllCanUnloadNow() { return !objects && !locks ? S_OK : S_FALSE; }
BOOL WINAPI DllMain(HINSTANCE instance, DWORD reason, LPVOID) {
    if (reason == DLL_PROCESS_ATTACH) { moduleHandle = instance; DisableThreadLibraryCalls(instance); }
    return TRUE;
}
