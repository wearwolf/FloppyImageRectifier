Public Class MfmSector

    Public ReadOnly SectorIdentifierBitIndex As Long
    Public ReadOnly TrackNumber As Byte
    Public ReadOnly SideNumber As Byte
    Public ReadOnly SectorNumber As Byte
    Public ReadOnly SectorSize As Integer
    Public ReadOnly SectorIdentifierChecksum As UShort
    Public ReadOnly SectorIdentifierCalculatedChecksum As UShort

    Public ReadOnly DataBitIndex As Long
    Public ReadOnly Data As List(Of Byte)
    Public ReadOnly DataChecksum As UShort
    Public ReadOnly DataCalculatedChecksum As UShort

    Public Sub New(sectorIdentifierBitIndex As Long, trackNumber As Byte, sideNumber As Byte, sectorNumber As Byte, sectorSize As Integer, sectorIdentifierChecksum As UShort,
                   sectorIdentifierCalculatedChecksum As UShort, dataBitIndex As Long, data As List(Of Byte), dataChecksum As UShort, dataCalculatedChecksum As UShort)
        Me.SectorIdentifierBitIndex = sectorIdentifierBitIndex
        Me.TrackNumber = trackNumber
        Me.SideNumber = sideNumber
        Me.SectorNumber = sectorNumber
        Me.SectorSize = sectorSize
        Me.SectorIdentifierChecksum = sectorIdentifierChecksum
        Me.SectorIdentifierCalculatedChecksum = sectorIdentifierCalculatedChecksum
        Me.DataBitIndex = dataBitIndex
        Me.Data = data
        Me.DataChecksum = dataChecksum
        Me.DataCalculatedChecksum = dataCalculatedChecksum
    End Sub

    Public ReadOnly Property SectorIdentifierChecksumValid As Boolean
        Get
            Return SectorIdentifierChecksum = SectorIdentifierCalculatedChecksum
        End Get
    End Property

    Public ReadOnly Property DataChecksumValid As Boolean
        Get
            Return DataChecksum = DataCalculatedChecksum
        End Get
    End Property
End Class
