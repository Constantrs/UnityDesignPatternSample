#ifndef UNIVERSAL_PIPELINE_INDIRECT_COMMON
#define UNIVERSAL_PIPELINE_INDIRECT_COMMON

struct boundData
{
    float3 boundsCenter;         // 3
    float3 boundsExtents;        // 6
};

struct instanceData
{
   uint matrixIndex;
};

#endif