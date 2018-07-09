#include <stdio.h>
#include <windows.h>
#include "ft_client.c"

typedef int (__declspec(dllexport) *f_TransmitData)(FTData*);
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
	