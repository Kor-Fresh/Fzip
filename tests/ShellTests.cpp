#ifndef UNICODE
#define UNICODE
#endif
#define _UNICODE
#include <windows.h>
#include <shobjidl.h>
#include <shlobj.h>
#include <shellapi.h>
#include <iostream>
#include <stdexcept>
#include <vector>
#include "../src/Fzip.Shell/CommandLine.h"

const CLSID clsid = {0xb09d5c80,0x77d3,0x4faf,{0xa4,0xb7,0x12,0x8f,0xe5,0xae,0x90,0x61}};
void Check(bool pass, const char* message) { if (!pass) throw std::runtime_error(message); }
IShellItemArray* Selection(const std::vector<std::wstring>& paths) {
    std::vector<PIDLIST_ABSOLUTE> ids;
    for (const auto& path : paths) {
        PIDLIST_ABSOLUTE id = nullptr;
        Check(SUCCEEDED(SHParseDisplayName(path.c_str(),nullptr,&id,0,nullptr)),"Parse shell path");
        ids.push_back(id);
    }
    IShellItemArray* items = nullptr;
    HRESULT hr = SHCreateShellItemArrayFromIDLists(static_cast<UINT>(ids.size()), const_cast<PCIDLIST_ABSOLUTE*>(ids.data()), &items);
    for (auto id : ids) CoTaskMemFree(id);
    Check(SUCCEEDED(hr),"Create shell selection"); return items;
}
int wmain(int argc, wchar_t** argv) {
    try {
        Check(argc == 3 || argc == 4,"DLL path and repository path required");
        CoInitializeEx(nullptr,COINIT_APARTMENTTHREADED);
        auto library = LoadLibraryW(argv[1]); Check(library != nullptr,"Load DLL");
        auto getClass = reinterpret_cast<HRESULT(WINAPI*)(REFCLSID,REFIID,void**)>(GetProcAddress(library,"DllGetClassObject"));
        auto canUnload = reinterpret_cast<HRESULT(WINAPI*)()>(GetProcAddress(library,"DllCanUnloadNow"));
        Check(getClass && canUnload && canUnload() == S_OK,"Exports and initial lifetime");
        IClassFactory* factory = nullptr;
        Check(SUCCEEDED(getClass(clsid,IID_IClassFactory,reinterpret_cast<void**>(&factory))),"Class factory");
        Check(canUnload() == S_FALSE,"Factory lifetime");
        IExplorerCommand* command = nullptr;
        Check(SUCCEEDED(factory->CreateInstance(nullptr,IID_IExplorerCommand,reinterpret_cast<void**>(&command))),"Command interface");
        factory->Release();
        GUID name; Check(SUCCEEDED(command->GetCanonicalName(&name)) && name == clsid,"Command identity");
        auto root = std::wstring(argv[2]);
        int context = 0;
        auto checkTitle = [&](IShellItemArray* items, const wchar_t* expected) {
            ++context;
            PWSTR title = nullptr; EXPCMDSTATE state;
            Check(SUCCEEDED(command->GetTitle(items,&title)),"Get title");
            bool match = std::wstring(title) == expected;
            if (!match) { std::cerr << "Context " << context << " title mismatch\n"; std::wcerr << title << L" != " << expected << L'\n'; }
            CoTaskMemFree(title); Check(match,"Context title");
            Check(SUCCEEDED(command->GetState(items,FALSE,&state)) && state == ECS_ENABLED,"Enabled context state");
            if(items) items->Release();
        };
        checkTitle(nullptr,L"Fzip으로 압축하기");
        checkTitle(Selection({root+L"\\README.md"}),L"Fzip으로 압축하기");
        checkTitle(Selection({root+L"\\tests"}),L"Fzip으로 압축하기");
        checkTitle(Selection({root+L"\\tests\\fixtures\\defaults.alz"}),L"Fzip으로 압축풀기");
        checkTitle(Selection({root+L"\\tests\\fixtures\\Rar.rar"}),L"Fzip으로 압축풀기");
        checkTitle(Selection({root+L"\\README.md",root+L"\\THIRD-PARTY-NOTICES.txt"}),L"Fzip으로 압축하기");
        std::vector<std::wstring> huge(129,root+L"\\README.md"); auto items=Selection(huge); EXPCMDSTATE state;
        Check(SUCCEEDED(command->GetState(items,FALSE,&state)) && state == ECS_HIDDEN,"Oversized selection hidden"); items->Release();
        for (const auto& argument : {L"",L"C:\\한글 폴더\\a.txt",L"C:\\folder\\",L"a\"b",L"a\\\"b",L"& calc.exe"}) {
            auto line = L"Fzip.exe " + QuoteArgument(argument); int count=0;
            auto parsed=CommandLineToArgvW(line.c_str(),&count);
            Check(parsed && count==2 && std::wstring(parsed[1])==argument,"argv quoting roundtrip"); LocalFree(parsed);
        }
        PWSTR icon=nullptr; Check(SUCCEEDED(command->GetIcon(nullptr,&icon)) && std::wstring(icon).find(L"archive.ico") != std::wstring::npos,"Icon"); CoTaskMemFree(icon);
        command->Release(); Check(canUnload()==S_OK,"Final COM lifetime");
        FreeLibrary(library);
        if (argc == 4 && std::wstring(argv[3]) == L"--registered") {
            IExplorerCommand* registered = nullptr;
            HRESULT hr = CoCreateInstance(clsid, nullptr, CLSCTX_LOCAL_SERVER, IID_IExplorerCommand, reinterpret_cast<void**>(&registered));
            if (FAILED(hr)) { std::cerr << "Packaged COM activation HRESULT=" << std::hex << hr << '\n'; }
            Check(SUCCEEDED(hr), "Registered package COM activation");
            PWSTR title=nullptr;
            Check(SUCCEEDED(registered->GetTitle(nullptr,&title)),"Registered title"); CoTaskMemFree(title);
            auto selected=Selection({root+L"\\README.md"});
            hr=registered->Invoke(selected,nullptr); selected->Release();
            registered->Release(); Check(SUCCEEDED(hr),"Registered Invoke launching Fzip");
            std::cout << "PASS registered package: out-of-process COM activation and Invoke\n";
        }
        CoUninitialize();
        std::cout << "PASS shell: COM lifecycle, 7 selection contexts, icon, 6 quoting roundtrips\n";
        return 0;
    } catch (const std::exception& e) { std::cerr<<e.what()<<'\n'; return 1; }
}
