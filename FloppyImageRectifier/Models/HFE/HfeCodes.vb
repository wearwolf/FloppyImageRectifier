Public Module HfeCodes

    Public Enum HfeStep As Byte
        DoubleStep = 0
        SingleStep = &HFF
    End Enum

    Public Enum HfeTrackEncoding As Byte
        ISOIBM_MFM_ENCODING = 0
        AMIGA_MFM_ENCODING = 1
        ISOIBM_FM_ENCODING = 2
        EMU_FM_ENCODING = 3
        UNKNOWN_ENCODING = &HFF
    End Enum

    Public Enum HfeFloppyInterfaceMode As Byte
        IBMPC_DD_FLOPPYMODE = 0
        IBMPC_HD_FLOPPYMODE = 1
        ATARIST_DD_FLOPPYMODE = 2
        ATARIST_HD_FLOPPYMODE = 3
        AMIGA_DD_FLOPPYMODE = 4
        AMIGA_HD_FLOPPYMODE = 5
        CPC_DD_FLOPPYMODE = 6
        GENERIC_SHUGGART_DD_FLOPPYMODE = 7
        IBMPC_ED_FLOPPYMODE = 8
        MSX2_DD_FLOPPYMODE = 9
        C64_DD_FLOPPYMODE = 10
        EMU_SHUGART_FLOPPYMODE = 11
        S950_DD_FLOPPYMODE = 12
        S950_HD_FLOPPYMODE = 13
        DISABLE_FLOPPYMODE = &HFF
    End Enum

End Module
