


typedef struct FTHeap__ {
    FTData data;
    int32_t GameID;
    union
    {
        unsigned char table[8];
        int32_t table_ints[2];
    };
    int32_t GameID2;
} volatile FTHeap;

typedef struct FTData__ {
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
} volatile FTData;
