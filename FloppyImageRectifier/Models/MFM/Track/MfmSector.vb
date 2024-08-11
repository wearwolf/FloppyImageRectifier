Public Class MfmSector

    Public ReadOnly IdentStartBitIndex As Long
    Public ReadOnly TrackNumber As Byte
    Public ReadOnly SideNumber As Byte
    Public ReadOnly SectorNumber As Byte
    Public ReadOnly SectorSize As Integer
    Public ReadOnly IdentChecksum As UShort
    Public ReadOnly IdentCalculatedChecksum As UShort
    Public ReadOnly IdentEndBitIndex As Long

    Public ReadOnly DataStartBitIndex As Long
    Public ReadOnly Data As List(Of Byte)
    Public ReadOnly DataChecksum As UShort
    Public ReadOnly DataCalculatedChecksum As UShort
    Public ReadOnly DataEndBitIndex As Long

    Public Sub New(identStartBitIndex As Long,
                   trackNumber As Byte,
                   sideNumber As Byte,
                   sectorNumber As Byte,
                   sectorSize As Integer,
                   identChecksum As UShort,
                   identCalculatedChecksum As UShort,
                   identEndBitIndex As Long,
                   dataStartBitIndex As Long,
                   data As List(Of Byte),
                   dataChecksum As UShort,
                   dataCalculatedChecksum As UShort,
                   dataEndBitIndex As Long)
        Me.IdentStartBitIndex = identStartBitIndex
        Me.TrackNumber = trackNumber
        Me.SideNumber = sideNumber
        Me.SectorNumber = sectorNumber
        Me.SectorSize = sectorSize
        Me.IdentChecksum = identChecksum
        Me.IdentCalculatedChecksum = identCalculatedChecksum
        Me.IdentEndBitIndex = identEndBitIndex
        Me.DataStartBitIndex = dataStartBitIndex
        Me.Data = data
        Me.DataChecksum = dataChecksum
        Me.DataCalculatedChecksum = dataCalculatedChecksum
        Me.DataEndBitIndex = dataEndBitIndex
    End Sub

    Public ReadOnly Property IdentChecksumValid As Boolean
        Get
            Return IdentChecksum = IdentCalculatedChecksum
        End Get
    End Property

    Public ReadOnly Property DataChecksumValid As Boolean
        Get
            Return DataChecksum = DataCalculatedChecksum
        End Get
    End Property
End Class
