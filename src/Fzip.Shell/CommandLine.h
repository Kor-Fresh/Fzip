#pragma once
#include <string>

// Quote a single Windows argv element, including trailing backslashes.
inline std::wstring QuoteArgument(const std::wstring& input) {
    std::wstring result = L"\"";
    size_t slashes = 0;
    for (wchar_t c : input) {
        if (c == L'\\') { ++slashes; continue; }
        result.append(c == L'"' ? slashes * 2 + 1 : slashes, L'\\');
        slashes = 0;
        result += c;
    }
    result.append(slashes * 2, L'\\');
    result += L'"';
    return result;
}
