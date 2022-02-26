// SUFTP-launcher.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
#include <iostream>
#include <string>
#include <vector>
#include <Windows.h>

using namespace std;

void exitOnKeyPress(int code) {
    exit(code);
}

string getFromCMD(wstring commandName, wstring commandLine, bool bOutput) {
    LPCWSTR cmd = commandName.c_str();
    LPCWSTR cmdLine = commandLine.c_str();
    BOOL ok = TRUE;
    HANDLE hStdInPipeRead = NULL;
    HANDLE hStdInPipeWrite = NULL;
    HANDLE hStdOutPipeRead = NULL;
    HANDLE hStdOutPipeWrite = NULL;
    
    SECURITY_ATTRIBUTES sa = { sizeof(SECURITY_ATTRIBUTES), NULL, TRUE };
    ok = CreatePipe(&hStdInPipeRead, &hStdInPipeWrite, &sa, 0);
    if (ok == FALSE) return "<CREATE_PIPE_ERROR>";
    ok = CreatePipe(&hStdOutPipeRead, &hStdOutPipeWrite, &sa, 0);
    if (ok == FALSE) return "<CREATE_PIPE_ERROR>";

    STARTUPINFO si = {};
    si.cb = sizeof(STARTUPINFO);
    si.dwFlags = STARTF_USESTDHANDLES;
    si.hStdError = hStdOutPipeWrite;
    si.hStdOutput = hStdOutPipeWrite;
    si.hStdInput = hStdInPipeRead;
    PROCESS_INFORMATION pi = {};
    LPCWSTR lpApplicationName = cmd;
    LPWSTR lpCommandLine = (LPWSTR)cmdLine;
    LPSECURITY_ATTRIBUTES lpProcessAttributes = NULL;
    LPSECURITY_ATTRIBUTES lpThreadAttribute = NULL;
    BOOL bInheritHandles = TRUE;
    DWORD dwCreationFlags = 0;
    LPVOID lpEnvironment = NULL;
    LPCWSTR lpCurrentDirectory = NULL;

    ok = CreateProcess(
        lpApplicationName,
        lpCommandLine,
        (LPSECURITY_ATTRIBUTES) NULL,
        (LPSECURITY_ATTRIBUTES) NULL,
        (BOOL) TRUE,
        (DWORD) 0,
        (LPVOID) NULL,
        (LPCWSTR) NULL,
        &si,
        &pi);

    if (ok == FALSE) return "<CREATE_PROCESS_ERROR>";

    CloseHandle(hStdOutPipeWrite);
    CloseHandle(hStdInPipeRead);

    char buf[1024 + 1] = {};
    string strOut = "";
    DWORD dwRead = 0;
    DWORD dwAvail = 0;
    ok = ReadFile(hStdOutPipeRead, buf, 1024, &dwRead, NULL);
    while (ok == TRUE) {
        buf[dwRead] = '\0';
        if (bOutput) OutputDebugStringA(buf);
        strOut += buf;
        if (bOutput) puts(buf);
        ok = ReadFile(hStdOutPipeRead, buf, 1024, &dwRead, NULL);
    }

    CloseHandle(hStdOutPipeRead);
    CloseHandle(hStdInPipeWrite);
    DWORD dwExitCode = 0;
    GetExitCodeProcess(pi.hProcess, &dwExitCode);
    return strOut;
}

string getFileVer(wstring fileName) {
    string result = "something didn't pass";
    DWORD verHandle = 0;
    UINT size = 0;
    LPBYTE lpBuffer = NULL;
    DWORD verSize = GetFileVersionInfoSize(fileName.c_str(), &verHandle);

    if (verSize != NULL) {
        LPSTR verData = new char[verSize];
        if (GetFileVersionInfo(fileName.c_str(), verHandle, verSize, verData)) {
            if (VerQueryValue(verData, L"\\", (VOID FAR * FAR*) & lpBuffer, &size)) {
                if (size)
                {
                    VS_FIXEDFILEINFO* verInfo = (VS_FIXEDFILEINFO*)lpBuffer;
                    if (verInfo->dwSignature == 0xfeef04bd) {
                        result= to_string((verInfo->dwFileVersionMS >> 16) & 0xffff)+"."+to_string((verInfo->dwFileVersionMS >> 0) & 0xffff)+"."+to_string((verInfo->dwFileVersionLS >> 16) & 0xffff) + "." + to_string((verInfo->dwFileVersionLS >> 0) & 0xffff);
                    }
                }
            }
        }
    }
    return result;
}

void tokenize(string const &str, const char delim, vector<string> &out) {
    size_t start;
    size_t end = 0;
    while ((start = str.find_first_not_of(delim, end)) != string::npos) {
        end = str.find(delim, start);
        out.push_back(str.substr(start, end - start));
    }
}

bool updatesAvailable() {
    bool updatesAvailable = false;
    wstring rootDir = L"C:\\Program Files\\SyncUp\\";
    wstring file[3];
    file[0] = L"SyncUp.exe";
    file[1] = L"SUFTPListener.exe";
    file[2] = L"SUFTP-all.exe";
    for (int i = 0; i < 3; i++) {
        string localResponse = getFileVer((rootDir + file[i]));
        string webResponse = getFromCMD(L"C:\\Windows\\System32\\cmd.exe", (L"/c C:\\Program^ Files\\SyncUp\\curl\\curl.exe -ks https://syncup.thatonetechcrew.net:8080/versions/check?assembly=" + file[i]), false);
        if (localResponse._Equal(webResponse))
            continue;
        else
        { 
            updatesAvailable = true;
            break;
        }
    }
    return updatesAvailable;
}
void doUpdates() {
    wstring rootDir = L"\"C:\\Program Files\\SyncUp\\";

    wstring file[3];
    file[0] = L"SyncUp.exe";
    file[1] = L"SUFTPListener.exe";
    file[2] = L"SUFTP-all.exe";
    for (int i = 0; i < 3; i++) {
        string webResponse = getFromCMD(L"C:\\Windows\\System32\\cmd.exe", 
            (L" /c C:\\\"Program Files\"\\SyncUp\\curl\\curl.exe https://syncup.thatonetechcrew.net/current/"+file[i]+ L" -o " + rootDir + file[i]+L"\""), false);
    }
}
int main(int argc, char** argv) 
{
    string search = "doUpdates";
    if (updatesAvailable())
        cout << "updates_available";
    else
        cout << "all_up_to_date";
    if (argc > 1) {
        if (search._Equal(argv[1]))
            doUpdates();
    }
}
