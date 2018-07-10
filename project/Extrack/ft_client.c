
#include <stdio.h>
#include <string.h>
#include <windows.h>

#include "fttypes.h"


#define FT_EXPORT(t) __declspec(dllexport) t

#define dbg_report(...) ((void)0)

static HANDLE hFTMemMap = 0;
static FTHeap* ipc_heap = 0;
static HANDLE ipc_mutex = 0;
static const char* dllVersion = "1.0.0.0";
static const char* dllProvider = "FreeTrack";

static BOOL impl_create_mapping(void)
{
    if (ipc_heap != NULL && ipc_mutex != NULL) {
        return TRUE;
	}

    hFTMemMap = CreateFileMappingA(INVALID_HANDLE_VALUE,
                                   NULL,
                                   PAGE_READWRITE,
                                   0,
                                   sizeof(FTHeap),
                                   (LPCSTR) FREETRACK_HEAP);
	
    if (hFTMemMap == NULL) {
		printf("failed to create file mapping: %d\n", GetLastError());
        return FALSE;
	}
	//printf("created file mapping\n");

    ipc_heap = (FTHeap*) MapViewOfFile(hFTMemMap, FILE_MAP_ALL_ACCESS, 0, 0, sizeof(FTHeap));
	if (ipc_heap == NULL) {
		printf("failed to map file to mem: %d\n", GetLastError());
		return FALSE;
	}
	//printf("mapped file to mem\n");
    ipc_mutex = CreateMutexA(NULL, FALSE, FREETRACK_MUTEX);
	if (ipc_mutex == NULL) {
		printf("failed to create sync: %d\n", GetLastError());
		return FALSE;
	}
	//printf("created mutex for synchronization\n");
    return TRUE;
}
static BOOL impl_open_mapping(void) {
	if (ipc_heap != NULL && ipc_mutex != NULL) {
		return TRUE;
	}
	hFTMemMap = OpenFileMappingA(FILE_MAP_ALL_ACCESS, FALSE, FREETRACK_HEAP);
	if (hFTMemMap == NULL) {
		printf("failed to open file mapping: %d\n", GetLastError());
		return FALSE;
	}
	//printf("opened file mapping\n");
	ipc_heap = (FTHeap*)MapViewOfFile(hFTMemMap, FILE_MAP_ALL_ACCESS, 0, 0, sizeof(FTHeap));
	if (ipc_heap == NULL) {
		printf("failed to map file to mem: %d\n", GetLastError());
		return FALSE;
	}
	//printf("mapped file to mem\n");
	ipc_mutex = OpenMutexA(SYNCHRONIZE, FALSE, FREETRACK_MUTEX);
	if (ipc_mutex == NULL) {
		printf("failed to open sync: %d\n", GetLastError());
		return FALSE;
	}
	//printf("opened mutex for synchronization\n");
	return TRUE;
}
	
FT_EXPORT(BOOL) FTPutData(FTData* data) {
	if (impl_create_mapping() == FALSE) {
		return FALSE;
	}
	if (ipc_mutex && WaitForSingleObject(ipc_mutex, 16) == WAIT_OBJECT_0) {
		//printf("moving data from memory to file\n");
		//memcpy(&ipc_heap->data, data, sizeof(FTData));
		CopyMemory((PVOID)&ipc_heap->data, data, sizeof(FTData));
		ipc_heap->data.DataID += 1;
		ReleaseMutex(ipc_mutex);
		//printf("finished moving data from memory to file\n");
	}
	return TRUE;
}
FT_EXPORT(BOOL) FTGetData(FTData* data)
{
    if (impl_open_mapping() == FALSE)
        return FALSE;

    if (ipc_mutex && WaitForSingleObject(ipc_mutex, 16) == WAIT_OBJECT_0) {
		//printf("moving data from file to memory\n");
        //memcpy(data, &ipc_heap->data, sizeof(FTData));
		CopyMemory(data, &ipc_heap->data, sizeof(FTData));
        if (ipc_heap->data.DataID > (1 << 29)) {
            ipc_heap->data.DataID = 0;
		}
        ReleaseMutex(ipc_mutex);
		//printf("finished moving data from file to memory\n");#
    }
    return TRUE;
}

/*
// For some mysterious reason, the previously existing function FTReportID has been changed to FTReportName, but with an integer as argument.
// The Delphi-code from the FreeTrack repo suggest a char * as argument, so it cost me an afternoon to figure it out (and keep ArmA2 from crashing).
// Thanks guys!
*/
FT_EXPORT(void) FTReportName( int name )
{
    dbg_report("FTReportName request (ID = %d).\n", name);
}

FT_EXPORT(void) FTReportID( int name )
{
    dbg_report("FTReportID request (ID = %d).\n", name);
}

FT_EXPORT(const char*) FTGetDllVersion(void)
{
    dbg_report("FTGetDllVersion request.\n");
    return dllVersion;
}

FT_EXPORT(const char*) FTProvider(void)
{
    dbg_report("FTProvider request.\n");
    return dllProvider;
}

#if defined _MSC_VER && !defined _WIN64
#pragma comment (linker, "/export:FTReportID=_FTReportID@4")
#pragma comment (linker, "/export:FTReportName=_FTReportName@4")
#pragma comment (linker, "/export:FTGetDllVersion=_FTGetDllVersion@0")
#pragma comment (linker, "/export:FTProvider=_FTProvider@0")
#pragma comment (linker, "/export:FTGetData=_FTGetData@4")
#endif