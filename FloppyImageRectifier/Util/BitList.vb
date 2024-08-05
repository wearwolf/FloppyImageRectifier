Public Class BitList

#Region "Fields"

    Private m_writeByte As Byte
    Private m_writeByteIndex As Integer
    Private m_writeByteBitIndex As Integer

    Private m_readByte As Byte
    Private m_readByteIndex As Integer
    Private m_readByteBitIndex As Integer

    Private m_saved As Boolean
    Private m_savedReadByte As Byte
    Private m_savedReadByteIndex As Integer
    Private m_savedReadByteBitIndex As Integer

    Private m_totalBitCount As Long

#End Region

#Region "Constructors"

    Public Sub New()
        Data = New List(Of Byte)()

        m_writeByte = 0
        m_writeByteIndex = 0
        m_writeByteBitIndex = 7
        m_totalBitCount = 0

        Data.Add(m_writeByte)

        m_readByte = 0
        m_readByteIndex = -1
        m_readByteBitIndex = 0

        m_saved = False
        m_savedReadByte = 0
        m_savedReadByteIndex = 0
        m_savedReadByteBitIndex = 0
    End Sub

    Public Sub New(dataBuffer As List(Of Byte), length As Integer)
        Me.New()
        Data = dataBuffer

        m_totalBitCount = length * 8
    End Sub

#End Region

#Region "Properties"

    Public ReadOnly Property Data As List(Of Byte)

    Public Property BitCount As Integer
        Get
            Return m_totalBitCount
        End Get
        Private Set
            m_totalBitCount = Value
        End Set
    End Property

    Public ReadOnly Property IsEof As Boolean
        Get
            Return Position >= m_totalBitCount
        End Get
    End Property

    Public ReadOnly Property Position As Long
        Get
            Return m_readByteIndex * 8 + (7 - m_readByteBitIndex)
        End Get
    End Property

#End Region

#Region "Public Methods"

    Public Sub AddBit(bit As Boolean)
        If bit Then
            m_writeByte += 1 << m_writeByteBitIndex
            Data(m_writeByteIndex) = m_writeByte
        End If

        m_writeByteBitIndex -= 1
        If m_writeByteBitIndex < 0 Then
            m_writeByte = 0
            m_writeByteBitIndex = 7
            Data.Add(m_writeByte)
            m_writeByteIndex += 1
        End If
        m_totalBitCount += 1
    End Sub

    Public Function ReadBit() As Boolean
        If IsEof Then
            Throw New InvalidOperationException("Reached end of stream")
        End If

        If m_readByteBitIndex = 0 Then
            m_readByteBitIndex = 7
            m_readByteIndex += 1
            m_readByte = Data(m_readByteIndex)
        Else
            m_readByteBitIndex -= 1
        End If

        Return (m_readByte And (1 << m_readByteBitIndex)) > 0
    End Function

    Public Sub SavePosition()
        m_savedReadByte = m_readByte
        m_savedReadByteIndex = m_readByteIndex
        m_savedReadByteBitIndex = m_readByteBitIndex
    End Sub

    Public Sub RestorePosition()
        m_readByte = m_savedReadByte
        m_readByteIndex = m_savedReadByteIndex
        m_readByteBitIndex = m_savedReadByteBitIndex
    End Sub

    Public Sub ResetRead()
        m_readByte = 0
        m_readByteIndex = -1
        m_readByteBitIndex = 0
    End Sub


    Friend Function MatchPercentage(testData As BitList) As Double
        Dim matches = 0
        Dim totalTransitions = 0

        Me.ResetRead()
        testData.ResetRead()
        While Not Me.IsEof AndAlso Not testData.IsEof
            Dim meZeroCount = 0
            While Not Me.IsEof AndAlso Me.ReadBit() = False
                meZeroCount += 1
            End While

            Dim testZeroCount = 0
            While Not testData.IsEof AndAlso testData.ReadBit() = False
                testZeroCount += 1
            End While

            If meZeroCount = testZeroCount Then
                matches += 1
            End If

            totalTransitions += 1
        End While

        Return matches / totalTransitions
    End Function

#End Region

End Class
