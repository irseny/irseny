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
	f_TransmitData putData = (f_TransmitData)GetProcAddress(h_TransLib, "FTPutData");
	if (!putData) {
		printf("Unable to load transmitter put function\n");
		return -1;
	}
	int signal;
	FTData data;
	memset(&data, 0, sizeof(FTData));
	int iter = 0;
	do {
		iter += 1;
		data.Yaw = iter;
		printf("Sending data %d\n", iter);
		if (putData(&data) == FALSE) {
			printf("Failed to send data\n");
		}
		signal = getchar();
	} while (signal != 'q');
	return 0;
}
	