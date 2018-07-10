/*#include <stdio.h>
#include <windows.h>
#include "ft_client.c"

typedef BOOL (__declspec(dllimport) *f_TransmitData)(FTData*);
int main(int argc, char** args) {
	HINSTANCE h_TransLib = LoadLibrary("libExtrack.dll");
	if (!h_TransLib) {
		printf("Unable to load transmitter library\n");
		return -1;
	}
	f_TransmitData getData = (f_TransmitData)GetProcAddress(h_TransLib, "FTGetData");
	if (!getData) {
		printf("Unable to load transmitter get function\n");
		return -1;
	}
	int signal;
	FTData data;
	
	do {
		signal = getchar();
		if (getData(&data) == FALSE) {
			printf("Failed to receive data\n");
		} else {
			printf("Received packed %d with content: %d\n", data.DataID, data.Yaw);
		}		
	} while (signal != 'q');
	return 0;
}
	*/
/*#include <windows.h>
#include <stdio.h>
#include <conio.h>
#include <tchar.h>
#pragma comment(lib, "user32.lib")

#define BUF_SIZE 256
TCHAR szName[]=TEXT("MyFileMappingObject");

int _tmain()
{
   HANDLE hMapFile;
   LPCTSTR pBuf;

   hMapFile = CreateFileMappingA(
                 INVALID_HANDLE_VALUE,    // use paging file
                 NULL,                    // default security
                 PAGE_READWRITE,          // read/write access
                 0,                       // maximum object size (high-order DWORD)
                 BUF_SIZE,                // maximum object size (low-order DWORD)
                 szName);                // name of mapping object

   if (hMapFile == NULL)
   {
      _tprintf(TEXT("Could not open file mapping object (%d).\n"),
             GetLastError());
      return 1;
   }

   pBuf = (LPTSTR) MapViewOfFile(hMapFile, // handle to map object
               FILE_MAP_READ,  // read/write permission
               0,
               0,
               BUF_SIZE);

   if (pBuf == NULL)
   {
      _tprintf(TEXT("Could not map view of file (%d).\n"),
             GetLastError());

      CloseHandle(hMapFile);

      return 1;
   }

   MessageBox(NULL, pBuf, TEXT("Process2"), MB_OK);

   UnmapViewOfFile(pBuf);

   CloseHandle(hMapFile);

   return 0;
}
*/
#include <windows.h>
#include <stdio.h>
#include <inttypes.h>

typedef struct {
    uint32_t DataID;
    int32_t CamWidth;
    int32_t CamHeight;
    /* virtual pose */
    float  Yaw;   /* positive yaw to the left */
    float  Pitch; /* positive pitch up */
    float  Roll;  /* positive roll to the left */
    float  X;
    float  Y;
    float  Z;
    /* raw pose with no smoothing, sensitivity, response curve etc. */
    float  RawYaw;
    float  RawPitch;
    float  RawRoll;
    float  RawX;
    float  RawY;
    float  RawZ;
    /* raw points, sorted by Y, origin top left corner */
    float  X1;
    float  Y1;
    float  X2;
    float  Y2;
    float  X3;
    float  Y3;
    float  X4;
    float  Y4;
} FTData_t;

int main(int argc, char** args) {
	HANDLE file = CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, sizeof(FTData_t), "FT_SharedMem");
	if (file == NULL) {
		printf("failed to create file mapping\n");
		return -1;
	}
	HANDLE sync = CreateMutexA(NULL, FALSE, "FT_Mutext");
	if (sync == NULL) {
		printf("failed to create sync\n");
		return -1;
	}
	FTData_t* map = (FTData_t*)MapViewOfFile(file, FILE_MAP_READ, 0, 0, sizeof(FTData_t));
	if (map == NULL) {
		printf("failed to map file to memory\n");
		return -1;
	}
	int input;
	do {
		FTData_t data;
		if (WaitForSingleObject(sync, 16) == WAIT_OBJECT_0) {
			CopyMemory(&data, map, sizeof(FTData_t));
			ReleaseMutex(sync);
		}
		printf("Received packed with id: %d\n", data.DataID);
		printf("Camera size: %dx%d\n", data.CamWidth, data.CamHeight);
		printf("Yaw: %f\n", data.Yaw);
		printf("Pitch: %f\n", data.Pitch);
		input = getchar();
	} while (input != 'q');
	UnmapViewOfFile(map);
	CloseHandle(sync);
	CloseHandle(file);
}
	