Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace FloppyImageRectifier.Tests
    <TestClass>
    Public Class BitListTests

#Region "Constructor"

        <TestMethod>
        Sub CreateEmptyBitList()
            Dim bitList = New BitList()

            Assert.AreEqual(0, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(0, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(0, bitList.Data.Count)
        End Sub

        <TestMethod>
        Sub CreateFromByteList()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H4}

            Dim bitList = New BitList(bytes, 24)

            Assert.AreEqual(24, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(24, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(3, bitList.Data.Count)
            Assert.AreEqual(&HA6, bitList.Data(0))
            Assert.AreEqual(&H79, bitList.Data(1))
            Assert.AreEqual(&H4, bitList.Data(2))
        End Sub

        <TestMethod>
        Sub CreateFromEmptyByteList()
            Dim bytes = New List(Of Byte) From {}

            Dim bitList = New BitList(bytes, 0)

            Assert.AreEqual(0, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(0, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(0, bitList.Data.Count)
        End Sub

        <TestMethod>
        Sub CreateFromPartialByteList()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}

            Dim bitList = New BitList(bytes, 20)

            Assert.AreEqual(20, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(20, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(3, bitList.Data.Count)
            Assert.AreEqual(&HA6, bitList.Data(0))
            Assert.AreEqual(&H79, bitList.Data(1))
            Assert.AreEqual(&H40, bitList.Data(2))
        End Sub

#End Region

#Region "AddBit"

        <TestMethod>
        Sub AddBitTrue()
            Dim bitList = New BitList()

            bitList.AddBit(True)

            Assert.AreEqual(1, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(1, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&H80, bitList.Data(0))
        End Sub

        <TestMethod>
        Sub AddBitFalse()
            Dim bitList = New BitList()

            bitList.AddBit(False)

            Assert.AreEqual(1, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(1, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&H0, bitList.Data(0))
        End Sub

        <TestMethod>
        Sub AddBitsOnBoundary()
            Dim bitList = New BitList()

            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(True)
            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(True)

            bitList.AddBit(True)
            bitList.AddBit(True)
            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(True)

            Assert.AreEqual(16, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(16, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(2, bitList.Data.Count)
            Assert.AreEqual(&H21, bitList.Data(0))
            Assert.AreEqual(&HC1, bitList.Data(1))
        End Sub

        <TestMethod>
        Sub AddBitsOffBoundary()
            Dim bitList = New BitList()

            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(True)
            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(True)

            bitList.AddBit(True)
            bitList.AddBit(True)
            bitList.AddBit(False)

            Assert.AreEqual(11, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(11, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(2, bitList.Data.Count)
            Assert.AreEqual(&H21, bitList.Data(0))
            Assert.AreEqual(&HC0, bitList.Data(1))
        End Sub

        <TestMethod>
        Sub AddBitsFromListOnBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}

            Dim bitList = New BitList(bytes, 20)

            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(True)
            bitList.AddBit(False)

            Assert.AreEqual(24, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(24, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(3, bitList.Data.Count)
            Assert.AreEqual(&HA6, bitList.Data(0))
            Assert.AreEqual(&H79, bitList.Data(1))
            Assert.AreEqual(&H42, bitList.Data(2))
        End Sub

        <TestMethod>
        Sub AddBitsFromListOffBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}

            Dim bitList = New BitList(bytes, 20)

            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(True)
            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(False)
            bitList.AddBit(True)

            Assert.AreEqual(28, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(28, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(4, bitList.Data.Count)
            Assert.AreEqual(&HA6, bitList.Data(0))
            Assert.AreEqual(&H79, bitList.Data(1))
            Assert.AreEqual(&H42, bitList.Data(2))
            Assert.AreEqual(&H10, bitList.Data(3))
        End Sub

#End Region

#Region "AddBitToStart"

        <TestMethod>
        Sub AddBitToStartTrue()
            Dim bitList = New BitList()

            bitList.AddBitToStart(True)

            Assert.AreEqual(1, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(1, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&H80, bitList.Data(0))
        End Sub

        <TestMethod>
        Sub AddBitToStartFalse()
            Dim bitList = New BitList()

            bitList.AddBitToStart(False)

            Assert.AreEqual(1, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(1, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&H0, bitList.Data(0))
        End Sub

        <TestMethod>
        Sub AddBitsToStartOnBoundary()
            Dim bitList = New BitList()

            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(True)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(True)
            bitList.AddBitToStart(True)

            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(True)
            bitList.AddBitToStart(True)
            bitList.AddBitToStart(True)
            bitList.AddBitToStart(False)

            Assert.AreEqual(16, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(16, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(2, bitList.Data.Count)
            Assert.AreEqual(&H70, bitList.Data(0))
            Assert.AreEqual(&HC4, bitList.Data(1))
        End Sub

        <TestMethod>
        Sub AddBitsToStartOfBoundary()
            Dim bitList = New BitList()

            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(True)

            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(True)
            bitList.AddBitToStart(True)
            bitList.AddBitToStart(True)
            bitList.AddBitToStart(False)

            Assert.AreEqual(11, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(11, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(2, bitList.Data.Count)
            Assert.AreEqual(&H70, bitList.Data(0))
            Assert.AreEqual(&H80, bitList.Data(1))
        End Sub

        <TestMethod>
        Sub AddBitsToStartFromListOnBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}

            Dim bitList = New BitList(bytes, 20)

            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(True)
            bitList.AddBitToStart(False)

            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(True)
            bitList.AddBitToStart(True)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(True)

            Assert.AreEqual(32, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(32, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(4, bitList.Data.Count)
            Assert.AreEqual(&H8C, bitList.Data(0))
            Assert.AreEqual(&H4A, bitList.Data(1))
            Assert.AreEqual(&H67, bitList.Data(2))
            Assert.AreEqual(&H94, bitList.Data(3))
        End Sub

        <TestMethod>
        Sub AddBitsToStartFromListOffBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}

            Dim bitList = New BitList(bytes, 20)

            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(True)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(False)
            bitList.AddBitToStart(True)

            Assert.AreEqual(28, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(28, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(4, bitList.Data.Count)
            Assert.AreEqual(&H84, bitList.Data(0))
            Assert.AreEqual(&HA6, bitList.Data(1))
            Assert.AreEqual(&H79, bitList.Data(2))
            Assert.AreEqual(&H40, bitList.Data(3))
        End Sub
#End Region

#Region "RemoveBitFromStart"

        <TestMethod>
        Sub RemoveBitFromStartException()
            Dim bitList = New BitList()

            Assert.ThrowsException(Of InvalidOperationException)(Function() bitList.RemoveBitFromStart())
        End Sub

        <TestMethod>
        Sub RemoveBitFromStartTrue()
            Dim bytes = New List(Of Byte) From {&H80}

            Dim bitList = New BitList(bytes, 1)

            Dim value = bitList.RemoveBitFromStart()

            Assert.AreEqual(True, value)
            Assert.AreEqual(0, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(0, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(0, bitList.Data.Count)
        End Sub

        <TestMethod>
        Sub RemoveBitFromStartFalse()
            Dim bytes = New List(Of Byte) From {&H0}

            Dim bitList = New BitList(bytes, 1)

            Dim value = bitList.RemoveBitFromStart()

            Assert.AreEqual(False, value)
            Assert.AreEqual(0, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(0, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(0, bitList.Data.Count)
        End Sub

        <TestMethod>
        Sub RemoveBitsFromStartOnBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}

            Dim bitList = New BitList(bytes, 20)

            Dim value0 = bitList.RemoveBitFromStart()
            Dim value1 = bitList.RemoveBitFromStart()
            Dim value2 = bitList.RemoveBitFromStart()
            Dim value3 = bitList.RemoveBitFromStart()

            Assert.AreEqual(True, value0)
            Assert.AreEqual(False, value1)
            Assert.AreEqual(True, value2)
            Assert.AreEqual(False, value3)
            Assert.AreEqual(16, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(16, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(2, bitList.Data.Count)
            Assert.AreEqual(&H67, bitList.Data(0))
            Assert.AreEqual(&H94, bitList.Data(1))
        End Sub

        <TestMethod>
        Sub RemoveBitsFromStartOffBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}

            Dim bitList = New BitList(bytes, 20)

            Dim value0 = bitList.RemoveBitFromStart()
            Dim value1 = bitList.RemoveBitFromStart()
            Dim value2 = bitList.RemoveBitFromStart()
            Dim value3 = bitList.RemoveBitFromStart()

            Dim value4 = bitList.RemoveBitFromStart()
            Dim value5 = bitList.RemoveBitFromStart()
            Dim value6 = bitList.RemoveBitFromStart()
            Dim value7 = bitList.RemoveBitFromStart()

            Assert.AreEqual(True, value0)
            Assert.AreEqual(False, value1)
            Assert.AreEqual(True, value2)
            Assert.AreEqual(False, value3)
            Assert.AreEqual(False, value4)
            Assert.AreEqual(True, value5)
            Assert.AreEqual(True, value6)
            Assert.AreEqual(False, value7)
            Assert.AreEqual(12, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(12, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(2, bitList.Data.Count)
            Assert.AreEqual(&H79, bitList.Data(0))
            Assert.AreEqual(&H40, bitList.Data(1))
        End Sub

#End Region

#Region "RemoveBitFromEnd"

        <TestMethod>
        Sub RemoveBitFromEndException()
            Dim bitList = New BitList()

            Assert.ThrowsException(Of InvalidOperationException)(Function() bitList.RemoveBitFromEnd())
        End Sub

        <TestMethod>
        Sub RemoveBitFromEndTrue()
            Dim bytes = New List(Of Byte) From {&H80}

            Dim bitList = New BitList(bytes, 1)

            Dim value = bitList.RemoveBitFromEnd()

            Assert.AreEqual(True, value)
            Assert.AreEqual(0, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(0, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(0, bitList.Data.Count)
        End Sub

        <TestMethod>
        Sub RemoveBitFromEndFalse()
            Dim bytes = New List(Of Byte) From {&H0}

            Dim bitList = New BitList(bytes, 1)

            Dim value = bitList.RemoveBitFromEnd()

            Assert.AreEqual(False, value)
            Assert.AreEqual(0, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(0, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(0, bitList.Data.Count)
        End Sub

        <TestMethod>
        Sub RemoveBitFromEndFullByteTrue()
            Dim bytes = New List(Of Byte) From {&HFF}

            Dim bitList = New BitList(bytes, 8)

            Dim value = bitList.RemoveBitFromEnd()

            Assert.AreEqual(True, value)
            Assert.AreEqual(7, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(7, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&HFE, bitList.Data(0))
        End Sub

        <TestMethod>
        Sub RemoveBitFromEndFullByteFalse()
            Dim bytes = New List(Of Byte) From {&HFE}

            Dim bitList = New BitList(bytes, 8)

            Dim value = bitList.RemoveBitFromEnd()

            Assert.AreEqual(False, value)
            Assert.AreEqual(7, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(7, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&HFE, bitList.Data(0))
        End Sub

        <TestMethod>
        Sub RemoveBitsFromEndOnBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}

            Dim bitList = New BitList(bytes, 20)

            Dim value1 = bitList.RemoveBitFromEnd()
            Dim value2 = bitList.RemoveBitFromEnd()
            Dim value3 = bitList.RemoveBitFromEnd()
            Dim value4 = bitList.RemoveBitFromEnd()

            Assert.AreEqual(False, value1)
            Assert.AreEqual(False, value2)
            Assert.AreEqual(True, value3)
            Assert.AreEqual(False, value4)
            Assert.AreEqual(16, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(16, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(2, bitList.Data.Count)
            Assert.AreEqual(&HA6, bitList.Data(0))
            Assert.AreEqual(&H79, bitList.Data(1))
        End Sub

        <TestMethod>
        Sub RemoveBitsFromEndOffBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}

            Dim bitList = New BitList(bytes, 20)

            Dim value1 = bitList.RemoveBitFromEnd()
            Dim value2 = bitList.RemoveBitFromEnd()
            Dim value3 = bitList.RemoveBitFromEnd()
            Dim value4 = bitList.RemoveBitFromEnd()
            Dim value5 = bitList.RemoveBitFromEnd()
            Dim value6 = bitList.RemoveBitFromEnd()
            Dim value7 = bitList.RemoveBitFromEnd()
            Dim value8 = bitList.RemoveBitFromEnd()

            Assert.AreEqual(False, value1)
            Assert.AreEqual(False, value2)
            Assert.AreEqual(True, value3)
            Assert.AreEqual(False, value4)
            Assert.AreEqual(True, value5)
            Assert.AreEqual(False, value6)
            Assert.AreEqual(False, value7)
            Assert.AreEqual(True, value8)
            Assert.AreEqual(12, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(12, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(2, bitList.Data.Count)
            Assert.AreEqual(&HA6, bitList.Data(0))
            Assert.AreEqual(&H70, bitList.Data(1))
        End Sub

#End Region

#Region "AddBitList"

        <TestMethod>
        Sub AddBitListStartOnBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H45}

            Dim bitList1 = New BitList(bytes, 24)
            Dim bitList2 = New BitList(bytes, 24)

            bitList1.AddBitList(bitList2)

            Assert.AreEqual(48, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(48, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(6, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H45, bitList1.Data(2))
            Assert.AreEqual(&HA6, bitList1.Data(3))
            Assert.AreEqual(&H79, bitList1.Data(4))
            Assert.AreEqual(&H45, bitList1.Data(5))

            Assert.AreEqual(24, bitList2.BitCount)
            Assert.AreEqual(24, bitList2.ReadPosition)
            Assert.AreEqual(24, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(3, bitList2.Data.Count)
            Assert.AreEqual(&HA6, bitList2.Data(0))
            Assert.AreEqual(&H79, bitList2.Data(1))
            Assert.AreEqual(&H45, bitList2.Data(2))
        End Sub

        <TestMethod>
        Sub AddBitListStartOffBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}

            Dim bitList1 = New BitList(bytes, 20)
            Dim bitList2 = New BitList(bytes, 20)

            bitList1.AddBitList(bitList2)

            Assert.AreEqual(40, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(40, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(5, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H4A, bitList1.Data(2))
            Assert.AreEqual(&H67, bitList1.Data(3))
            Assert.AreEqual(&H94, bitList1.Data(4))

            Assert.AreEqual(20, bitList2.BitCount)
            Assert.AreEqual(20, bitList2.ReadPosition)
            Assert.AreEqual(20, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(3, bitList2.Data.Count)
            Assert.AreEqual(&HA6, bitList2.Data(0))
            Assert.AreEqual(&H79, bitList2.Data(1))
            Assert.AreEqual(&H40, bitList2.Data(2))
        End Sub

        <TestMethod>
        Sub AddBitListEndOnBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 20)

            Dim bytes2 = New List(Of Byte) From {&HC6, &H20}
            Dim bitList2 = New BitList(bytes2, 12)

            bitList1.AddBitList(bitList2)

            Assert.AreEqual(32, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(32, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(4, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H4C, bitList1.Data(2))
            Assert.AreEqual(&H62, bitList1.Data(3))

            Assert.AreEqual(12, bitList2.BitCount)
            Assert.AreEqual(12, bitList2.ReadPosition)
            Assert.AreEqual(12, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(2, bitList2.Data.Count)
            Assert.AreEqual(&HC6, bitList2.Data(0))
            Assert.AreEqual(&H20, bitList2.Data(1))
        End Sub

        <TestMethod>
        Sub AddBitListEndOffBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 20)

            Dim bytes2 = New List(Of Byte) From {&HC6, &H27}
            Dim bitList2 = New BitList(bytes2, 16)

            bitList1.AddBitList(bitList2)

            Assert.AreEqual(36, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(36, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(5, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H4C, bitList1.Data(2))
            Assert.AreEqual(&H62, bitList1.Data(3))
            Assert.AreEqual(&H70, bitList1.Data(4))

            Assert.AreEqual(16, bitList2.BitCount)
            Assert.AreEqual(16, bitList2.ReadPosition)
            Assert.AreEqual(16, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(2, bitList2.Data.Count)
            Assert.AreEqual(&HC6, bitList2.Data(0))
            Assert.AreEqual(&H27, bitList2.Data(1))
        End Sub

#End Region

#Region "ReadBit"

        <TestMethod>
        Sub ReadBitException()
            Dim bitList = New BitList()

            Assert.ThrowsException(Of InvalidOperationException)(Function() bitList.ReadBit())
        End Sub

        <TestMethod>
        Sub ReadBitTrue()
            Dim bytes = New List(Of Byte) From {&H80}

            Dim bitList = New BitList(bytes, 1)

            Dim value = bitList.ReadBit()

            Assert.AreEqual(True, value)
            Assert.AreEqual(1, bitList.BitCount)
            Assert.AreEqual(1, bitList.ReadPosition)
            Assert.AreEqual(1, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&H80, bitList.Data(0))
        End Sub

        <TestMethod>
        Sub ReadBitFalse()
            Dim bytes = New List(Of Byte) From {&H0}

            Dim bitList = New BitList(bytes, 1)

            Dim value = bitList.ReadBit()

            Assert.AreEqual(False, value)
            Assert.AreEqual(1, bitList.BitCount)
            Assert.AreEqual(1, bitList.ReadPosition)
            Assert.AreEqual(1, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&H0, bitList.Data(0))
        End Sub

        <TestMethod>
        Sub ReadBitsOnBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}

            Dim bitList = New BitList(bytes, 20)

            Dim value0 = bitList.ReadBit()
            Dim value1 = bitList.ReadBit()
            Dim value2 = bitList.ReadBit()
            Dim value3 = bitList.ReadBit()

            Assert.AreEqual(True, value0)
            Assert.AreEqual(False, value1)
            Assert.AreEqual(True, value2)
            Assert.AreEqual(False, value3)
            Assert.AreEqual(20, bitList.BitCount)
            Assert.AreEqual(4, bitList.ReadPosition)
            Assert.AreEqual(20, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(3, bitList.Data.Count)
            Assert.AreEqual(&HA6, bitList.Data(0))
            Assert.AreEqual(&H79, bitList.Data(1))
            Assert.AreEqual(&H40, bitList.Data(2))
        End Sub

        <TestMethod>
        Sub ReadBitsOffBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}

            Dim bitList = New BitList(bytes, 20)

            Dim value0 = bitList.ReadBit()
            Dim value1 = bitList.ReadBit()
            Dim value2 = bitList.ReadBit()
            Dim value3 = bitList.ReadBit()

            Dim value4 = bitList.ReadBit()
            Dim value5 = bitList.ReadBit()
            Dim value6 = bitList.ReadBit()
            Dim value7 = bitList.ReadBit()

            Assert.AreEqual(True, value0)
            Assert.AreEqual(False, value1)
            Assert.AreEqual(True, value2)
            Assert.AreEqual(False, value3)
            Assert.AreEqual(False, value4)
            Assert.AreEqual(True, value5)
            Assert.AreEqual(True, value6)
            Assert.AreEqual(False, value7)
            Assert.AreEqual(20, bitList.BitCount)
            Assert.AreEqual(8, bitList.ReadPosition)
            Assert.AreEqual(20, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(3, bitList.Data.Count)
            Assert.AreEqual(&HA6, bitList.Data(0))
            Assert.AreEqual(&H79, bitList.Data(1))
            Assert.AreEqual(&H40, bitList.Data(2))
        End Sub

#End Region

#Region "SavePosition"

        <TestMethod>
        Sub SavePosition()
            Dim bytes = New List(Of Byte) From {&H79}

            Dim bitList = New BitList(bytes, 8)

            Dim value1 = bitList.ReadBit()
            Dim value2 = bitList.ReadBit()
            Dim value3 = bitList.ReadBit()
            bitList.SavePosition()

            Assert.AreEqual(False, value1)
            Assert.AreEqual(True, value2)
            Assert.AreEqual(True, value3)
            Assert.AreEqual(8, bitList.BitCount)
            Assert.AreEqual(3, bitList.ReadPosition)
            Assert.AreEqual(8, bitList.WritePosition)
            Assert.AreEqual(3, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&H79, bitList.Data(0))
        End Sub

        <TestMethod>
        Sub SavePositionReadAfter()
            Dim bytes = New List(Of Byte) From {&H79}

            Dim bitList = New BitList(bytes, 8)

            Dim value1 = bitList.ReadBit()
            Dim value2 = bitList.ReadBit()
            Dim value3 = bitList.ReadBit()
            bitList.SavePosition()
            Dim value4 = bitList.ReadBit()
            Dim value5 = bitList.ReadBit()

            Assert.AreEqual(False, value1)
            Assert.AreEqual(True, value2)
            Assert.AreEqual(True, value3)
            Assert.AreEqual(True, value4)
            Assert.AreEqual(True, value5)
            Assert.AreEqual(8, bitList.BitCount)
            Assert.AreEqual(5, bitList.ReadPosition)
            Assert.AreEqual(8, bitList.WritePosition)
            Assert.AreEqual(3, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&H79, bitList.Data(0))
        End Sub

#End Region

#Region "RestorePosition"

        <TestMethod>
        Sub RestorePosition()
            Dim bytes = New List(Of Byte) From {&H79}

            Dim bitList = New BitList(bytes, 8)

            Dim value1 = bitList.ReadBit()
            Dim value2 = bitList.ReadBit()
            Dim value3 = bitList.ReadBit()
            bitList.SavePosition()
            Dim value4 = bitList.ReadBit()
            Dim value5 = bitList.ReadBit()
            bitList.RestorePosition()

            Assert.AreEqual(False, value1)
            Assert.AreEqual(True, value2)
            Assert.AreEqual(True, value3)
            Assert.AreEqual(True, value4)
            Assert.AreEqual(True, value5)
            Assert.AreEqual(8, bitList.BitCount)
            Assert.AreEqual(3, bitList.ReadPosition)
            Assert.AreEqual(8, bitList.WritePosition)
            Assert.AreEqual(3, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&H79, bitList.Data(0))
        End Sub

        <TestMethod>
        Sub RestorePositionReadAfter()
            Dim bytes = New List(Of Byte) From {&H79}

            Dim bitList = New BitList(bytes, 8)

            Dim value1 = bitList.ReadBit()
            Dim value2 = bitList.ReadBit()
            Dim value3 = bitList.ReadBit()
            bitList.SavePosition()
            Dim value4 = bitList.ReadBit()
            Dim value5 = bitList.ReadBit()
            bitList.RestorePosition()

            Dim value6 = bitList.ReadBit()
            Dim value7 = bitList.ReadBit()

            Assert.AreEqual(False, value1)
            Assert.AreEqual(True, value2)
            Assert.AreEqual(True, value3)
            Assert.AreEqual(True, value4)
            Assert.AreEqual(True, value5)
            Assert.AreEqual(True, value6)
            Assert.AreEqual(True, value7)
            Assert.AreEqual(8, bitList.BitCount)
            Assert.AreEqual(5, bitList.ReadPosition)
            Assert.AreEqual(8, bitList.WritePosition)
            Assert.AreEqual(3, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&H79, bitList.Data(0))
        End Sub

#End Region

#Region "ResetRead"

        <TestMethod>
        Sub ResetRead()
            Dim bytes = New List(Of Byte) From {&H79}

            Dim bitList = New BitList(bytes, 8)

            Dim value1 = bitList.ReadBit()
            Dim value2 = bitList.ReadBit()
            Dim value3 = bitList.ReadBit()
            bitList.ResetRead()

            Assert.AreEqual(False, value1)
            Assert.AreEqual(True, value2)
            Assert.AreEqual(True, value3)

            Assert.AreEqual(8, bitList.BitCount)
            Assert.AreEqual(0, bitList.ReadPosition)
            Assert.AreEqual(8, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&H79, bitList.Data(0))
        End Sub

        <TestMethod>
        Sub ResetReadReadAfter()
            Dim bytes = New List(Of Byte) From {&H79}

            Dim bitList = New BitList(bytes, 8)

            Dim value1 = bitList.ReadBit()
            Dim value2 = bitList.ReadBit()
            Dim value3 = bitList.ReadBit()
            bitList.ResetRead()
            Dim value4 = bitList.ReadBit()
            Dim value5 = bitList.ReadBit()

            Assert.AreEqual(False, value1)
            Assert.AreEqual(True, value2)
            Assert.AreEqual(True, value3)
            Assert.AreEqual(False, value4)
            Assert.AreEqual(True, value5)

            Assert.AreEqual(8, bitList.BitCount)
            Assert.AreEqual(2, bitList.ReadPosition)
            Assert.AreEqual(8, bitList.WritePosition)
            Assert.AreEqual(0, bitList.SavedPosition)
            Assert.AreEqual(1, bitList.Data.Count)
            Assert.AreEqual(&H79, bitList.Data(0))
        End Sub

#End Region

#Region "MatchPercentage"

#End Region

#Region "Find"

        <TestMethod>
        Sub FindFromStart()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 20)

            Dim bytes2 = New List(Of Byte) From {&HA0}
            Dim bitList2 = New BitList(bytes2, 4)

            Dim index = bitList1.Find(0, 19, bitList2)

            Assert.AreEqual(0, index)
            Assert.AreEqual(20, bitList1.BitCount)
            Assert.AreEqual(4, bitList1.ReadPosition)
            Assert.AreEqual(20, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H40, bitList1.Data(2))

            Assert.AreEqual(4, bitList2.BitCount)
            Assert.AreEqual(4, bitList2.ReadPosition)
            Assert.AreEqual(4, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(1, bitList2.Data.Count)
            Assert.AreEqual(&HA0, bitList2.Data(0))
        End Sub

        <TestMethod>
        Sub FindFromStartAfter()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 20)

            Dim bytes2 = New List(Of Byte) From {&H60}
            Dim bitList2 = New BitList(bytes2, 4)

            Dim index = bitList1.Find(0, 19, bitList2)

            Assert.AreEqual(4, index)
            Assert.AreEqual(20, bitList1.BitCount)
            Assert.AreEqual(8, bitList1.ReadPosition)
            Assert.AreEqual(20, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H40, bitList1.Data(2))

            Assert.AreEqual(4, bitList2.BitCount)
            Assert.AreEqual(4, bitList2.ReadPosition)
            Assert.AreEqual(4, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(1, bitList2.Data.Count)
            Assert.AreEqual(&H60, bitList2.Data(0))
        End Sub

        <TestMethod>
        Sub FindFromStartNotFound()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 20)

            Dim bytes2 = New List(Of Byte) From {&H29}
            Dim bitList2 = New BitList(bytes2, 8)

            Dim index = bitList1.Find(0, 19, bitList2)

            Assert.AreEqual(-1, index)
            Assert.AreEqual(20, bitList1.BitCount)
            Assert.AreEqual(20, bitList1.ReadPosition)
            Assert.AreEqual(20, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H40, bitList1.Data(2))

            Assert.AreEqual(8, bitList2.BitCount)
            Assert.AreEqual(1, bitList2.ReadPosition)
            Assert.AreEqual(8, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(1, bitList2.Data.Count)
            Assert.AreEqual(&H29, bitList2.Data(0))
        End Sub

        <TestMethod>
        Sub FindFromStartNotFoundAfterMax()
            Dim bytes = New List(Of Byte) From {&HA6, &H72, &H90}
            Dim bitList1 = New BitList(bytes, 20)

            Dim bytes2 = New List(Of Byte) From {&H29}
            Dim bitList2 = New BitList(bytes2, 8)

            Dim index = bitList1.Find(0, 8, bitList2)

            Assert.AreEqual(-1, index)
            Assert.AreEqual(20, bitList1.BitCount)
            Assert.AreEqual(10, bitList1.ReadPosition)
            Assert.AreEqual(20, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H72, bitList1.Data(1))
            Assert.AreEqual(&H90, bitList1.Data(2))

            Assert.AreEqual(8, bitList2.BitCount)
            Assert.AreEqual(2, bitList2.ReadPosition)
            Assert.AreEqual(8, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(1, bitList2.Data.Count)
            Assert.AreEqual(&H29, bitList2.Data(0))
        End Sub

        <TestMethod>
        Sub FindFromMiddle()
            Dim bytes = New List(Of Byte) From {&H79, &H79, &HA6}
            Dim bitList1 = New BitList(bytes, 24)

            Dim bytes2 = New List(Of Byte) From {&H79}
            Dim bitList2 = New BitList(bytes2, 8)

            Dim index = bitList1.Find(1, 23, bitList2)

            Assert.AreEqual(8, index)
            Assert.AreEqual(24, bitList1.BitCount)
            Assert.AreEqual(16, bitList1.ReadPosition)
            Assert.AreEqual(24, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&H79, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&HA6, bitList1.Data(2))

            Assert.AreEqual(8, bitList2.BitCount)
            Assert.AreEqual(8, bitList2.ReadPosition)
            Assert.AreEqual(8, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(1, bitList2.Data.Count)
            Assert.AreEqual(&H79, bitList2.Data(0))
        End Sub

        <TestMethod>
        Sub FindFromMiddleAfter()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &HA6}
            Dim bitList1 = New BitList(bytes, 24)

            Dim bytes2 = New List(Of Byte) From {&HA6}
            Dim bitList2 = New BitList(bytes2, 8)

            Dim index = bitList1.Find(1, 23, bitList2)

            Assert.AreEqual(16, index)
            Assert.AreEqual(24, bitList1.BitCount)
            Assert.AreEqual(24, bitList1.ReadPosition)
            Assert.AreEqual(24, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&HA6, bitList1.Data(2))

            Assert.AreEqual(8, bitList2.BitCount)
            Assert.AreEqual(8, bitList2.ReadPosition)
            Assert.AreEqual(8, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(1, bitList2.Data.Count)
            Assert.AreEqual(&HA6, bitList2.Data(0))
        End Sub

        <TestMethod>
        Sub FindFromMiddleNotFound()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 24)

            Dim bytes2 = New List(Of Byte) From {&HA6}
            Dim bitList2 = New BitList(bytes2, 8)

            Dim index = bitList1.Find(1, 23, bitList2)

            Assert.AreEqual(-1, index)
            Assert.AreEqual(24, bitList1.BitCount)
            Assert.AreEqual(24, bitList1.ReadPosition)
            Assert.AreEqual(24, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H40, bitList1.Data(2))

            Assert.AreEqual(8, bitList2.BitCount)
            Assert.AreEqual(1, bitList2.ReadPosition)
            Assert.AreEqual(8, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(1, bitList2.Data.Count)
            Assert.AreEqual(&HA6, bitList2.Data(0))
        End Sub

        <TestMethod>
        Sub FindFromMiddleNotFoundAfterMax()
            Dim bytes = New List(Of Byte) From {&HA6, &H72, &H90}
            Dim bitList1 = New BitList(bytes, 24)

            Dim bytes2 = New List(Of Byte) From {&H29}
            Dim bitList2 = New BitList(bytes2, 8)

            Dim index = bitList1.Find(1, 8, bitList2)

            Assert.AreEqual(-1, index)
            Assert.AreEqual(24, bitList1.BitCount)
            Assert.AreEqual(10, bitList1.ReadPosition)
            Assert.AreEqual(24, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H72, bitList1.Data(1))
            Assert.AreEqual(&H90, bitList1.Data(2))

            Assert.AreEqual(8, bitList2.BitCount)
            Assert.AreEqual(2, bitList2.ReadPosition)
            Assert.AreEqual(8, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(1, bitList2.Data.Count)
            Assert.AreEqual(&H29, bitList2.Data(0))
        End Sub

#End Region

#Region "Slice"

        <TestMethod>
        Sub SliceFromStartOffBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 20)

            Dim bitList2 = bitList1.Slice(0, 4)

            Assert.AreEqual(20, bitList1.BitCount)
            Assert.AreEqual(4, bitList1.ReadPosition)
            Assert.AreEqual(20, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H40, bitList1.Data(2))

            Assert.AreEqual(4, bitList2.BitCount)
            Assert.AreEqual(0, bitList2.ReadPosition)
            Assert.AreEqual(4, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(1, bitList2.Data.Count)
            Assert.AreEqual(&HA0, bitList2.Data(0))
        End Sub

        <TestMethod>
        Sub SliceFromStartOnBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 20)

            Dim bitList2 = bitList1.Slice(0, 16)

            Assert.AreEqual(20, bitList1.BitCount)
            Assert.AreEqual(16, bitList1.ReadPosition)
            Assert.AreEqual(20, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H40, bitList1.Data(2))

            Assert.AreEqual(16, bitList2.BitCount)
            Assert.AreEqual(0, bitList2.ReadPosition)
            Assert.AreEqual(16, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(2, bitList2.Data.Count)
            Assert.AreEqual(&HA6, bitList2.Data(0))
            Assert.AreEqual(&H79, bitList2.Data(1))
        End Sub

        <TestMethod>
        Sub SliceFromMiddleOffBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 20)

            Dim bitList2 = bitList1.Slice(12, 4)

            Assert.AreEqual(20, bitList1.BitCount)
            Assert.AreEqual(16, bitList1.ReadPosition)
            Assert.AreEqual(20, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H40, bitList1.Data(2))

            Assert.AreEqual(4, bitList2.BitCount)
            Assert.AreEqual(0, bitList2.ReadPosition)
            Assert.AreEqual(4, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(1, bitList2.Data.Count)
            Assert.AreEqual(&H90, bitList2.Data(0))
        End Sub

        <TestMethod>
        Sub SliceFromMiddleOnBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 20)

            Dim bitList2 = bitList1.Slice(4, 16)

            Assert.AreEqual(20, bitList1.BitCount)
            Assert.AreEqual(20, bitList1.ReadPosition)
            Assert.AreEqual(20, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H40, bitList1.Data(2))

            Assert.AreEqual(16, bitList2.BitCount)
            Assert.AreEqual(0, bitList2.ReadPosition)
            Assert.AreEqual(16, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(2, bitList2.Data.Count)
            Assert.AreEqual(&H67, bitList2.Data(0))
            Assert.AreEqual(&H94, bitList2.Data(1))
        End Sub

#End Region

#Region "ShiftBitsFromStartOf"

        <TestMethod>
        Sub ShiftBitFromStartOf()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 24)

            Dim bytes2 = New List(Of Byte) From {&HA6}
            Dim bitList2 = New BitList(bytes2, 8)

            bitList1.ShiftBitsFromStartOf(bitList2, 1)

            Assert.AreEqual(25, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(25, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(4, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H40, bitList1.Data(2))
            Assert.AreEqual(&H80, bitList1.Data(3))

            Assert.AreEqual(7, bitList2.BitCount)
            Assert.AreEqual(0, bitList2.ReadPosition)
            Assert.AreEqual(7, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(1, bitList2.Data.Count)
            Assert.AreEqual(&H4C, bitList2.Data(0))
        End Sub

        <TestMethod>
        Sub ShiftBitsFromStartOf()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 24)

            Dim bytes2 = New List(Of Byte) From {&HA6}
            Dim bitList2 = New BitList(bytes2, 8)

            bitList1.ShiftBitsFromStartOf(bitList2, 4)

            Assert.AreEqual(28, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(28, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(4, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H40, bitList1.Data(2))
            Assert.AreEqual(&HA0, bitList1.Data(3))

            Assert.AreEqual(4, bitList2.BitCount)
            Assert.AreEqual(0, bitList2.ReadPosition)
            Assert.AreEqual(4, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(1, bitList2.Data.Count)
            Assert.AreEqual(&H60, bitList2.Data(0))
        End Sub

        <TestMethod>
        Sub ShiftBitsFromStartOfMultipleBytes()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 24)

            Dim bytes2 = New List(Of Byte) From {&HA6, &HD6, &H1F}
            Dim bitList2 = New BitList(bytes2, 24)

            bitList1.ShiftBitsFromStartOf(bitList2, 12)

            Assert.AreEqual(36, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(36, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(5, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H40, bitList1.Data(2))
            Assert.AreEqual(&HA6, bitList1.Data(3))
            Assert.AreEqual(&HD0, bitList1.Data(4))

            Assert.AreEqual(12, bitList2.BitCount)
            Assert.AreEqual(0, bitList2.ReadPosition)
            Assert.AreEqual(12, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(2, bitList2.Data.Count)
            Assert.AreEqual(&H61, bitList2.Data(0))
            Assert.AreEqual(&HF0, bitList2.Data(1))
        End Sub

        <TestMethod>
        Sub ShiftBitsFromStartOfMultipleBytesOffBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 20)

            Dim bytes2 = New List(Of Byte) From {&HA6, &HD6, &H1F}
            Dim bitList2 = New BitList(bytes2, 24)

            bitList1.ShiftBitsFromStartOf(bitList2, 12)

            Assert.AreEqual(32, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(32, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(4, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H4A, bitList1.Data(2))
            Assert.AreEqual(&H6D, bitList1.Data(3))

            Assert.AreEqual(12, bitList2.BitCount)
            Assert.AreEqual(0, bitList2.ReadPosition)
            Assert.AreEqual(12, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(2, bitList2.Data.Count)
            Assert.AreEqual(&H61, bitList2.Data(0))
            Assert.AreEqual(&HF0, bitList2.Data(1))
        End Sub
#End Region

#Region "ShiftBitsToStartOf"

        <TestMethod>
        Sub ShiftBitToStartOf()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 24)

            Dim bytes2 = New List(Of Byte) From {&HA6}
            Dim bitList2 = New BitList(bytes2, 8)

            bitList1.ShiftBitsToStartOf(bitList2, 1)

            Assert.AreEqual(23, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(23, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H40, bitList1.Data(2))

            Assert.AreEqual(9, bitList2.BitCount)
            Assert.AreEqual(0, bitList2.ReadPosition)
            Assert.AreEqual(9, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(2, bitList2.Data.Count)
            Assert.AreEqual(&H53, bitList2.Data(0))
            Assert.AreEqual(&H0, bitList2.Data(1))
        End Sub

        <TestMethod>
        Sub ShiftBitsToStartOf()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 24)

            Dim bytes2 = New List(Of Byte) From {&HA6}
            Dim bitList2 = New BitList(bytes2, 8)

            bitList1.ShiftBitsToStartOf(bitList2, 4)

            Assert.AreEqual(20, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(20, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H79, bitList1.Data(1))
            Assert.AreEqual(&H40, bitList1.Data(2))

            Assert.AreEqual(12, bitList2.BitCount)
            Assert.AreEqual(0, bitList2.ReadPosition)
            Assert.AreEqual(12, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(2, bitList2.Data.Count)
            Assert.AreEqual(&HA, bitList2.Data(0))
            Assert.AreEqual(&H60, bitList2.Data(1))
        End Sub

        <TestMethod>
        Sub ShiftBitsToStartOfMultipleBytes()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 20)

            Dim bytes2 = New List(Of Byte) From {&HA6, &HD6, &H10}
            Dim bitList2 = New BitList(bytes2, 20)

            bitList1.ShiftBitsToStartOf(bitList2, 12)

            Assert.AreEqual(8, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(8, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(1, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))

            Assert.AreEqual(32, bitList2.BitCount)
            Assert.AreEqual(0, bitList2.ReadPosition)
            Assert.AreEqual(32, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(4, bitList2.Data.Count)
            Assert.AreEqual(&H79, bitList2.Data(0))
            Assert.AreEqual(&H4A, bitList2.Data(1))
            Assert.AreEqual(&H6D, bitList2.Data(2))
            Assert.AreEqual(&H61, bitList2.Data(3))
        End Sub

        <TestMethod>
        Sub ShiftBitsToStartOfMultipleBytesOffBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 24)

            Dim bytes2 = New List(Of Byte) From {&HA6, &HD6, &H1F}
            Dim bitList2 = New BitList(bytes2, 24)

            bitList1.ShiftBitsToStartOf(bitList2, 12)

            Assert.AreEqual(12, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(12, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(2, bitList1.Data.Count)
            Assert.AreEqual(&HA6, bitList1.Data(0))
            Assert.AreEqual(&H70, bitList1.Data(1))

            Assert.AreEqual(36, bitList2.BitCount)
            Assert.AreEqual(0, bitList2.ReadPosition)
            Assert.AreEqual(36, bitList2.WritePosition)
            Assert.AreEqual(0, bitList2.SavedPosition)
            Assert.AreEqual(5, bitList2.Data.Count)
            Assert.AreEqual(&H94, bitList2.Data(0))
            Assert.AreEqual(&HA, bitList2.Data(1))
            Assert.AreEqual(&H6D, bitList2.Data(2))
            Assert.AreEqual(&H61, bitList2.Data(3))
            Assert.AreEqual(&HF0, bitList2.Data(4))
        End Sub

#End Region

#Region "RotateRight"

        <TestMethod>
        Sub RotateRightOne()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 24)

            bitList1.RotateRight(1)

            Assert.AreEqual(24, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(24, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&H53, bitList1.Data(0))
            Assert.AreEqual(&H3C, bitList1.Data(1))
            Assert.AreEqual(&HA0, bitList1.Data(2))
        End Sub

        <TestMethod>
        Sub RotateRightNibble()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 24)

            bitList1.RotateRight(4)

            Assert.AreEqual(24, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(24, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&HA, bitList1.Data(0))
            Assert.AreEqual(&H67, bitList1.Data(1))
            Assert.AreEqual(&H94, bitList1.Data(2))
        End Sub

        <TestMethod>
        Sub RotateRightMultipleBytesOffBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 20)

            bitList1.RotateRight(12)

            Assert.AreEqual(20, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(20, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&H79, bitList1.Data(0))
            Assert.AreEqual(&H4A, bitList1.Data(1))
            Assert.AreEqual(&H60, bitList1.Data(2))
        End Sub

        <TestMethod>
        Sub RotateRightMultipleBytes()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 24)

            bitList1.RotateRight(12)

            Assert.AreEqual(24, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(24, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&H94, bitList1.Data(0))
            Assert.AreEqual(&HA, bitList1.Data(1))
            Assert.AreEqual(&H67, bitList1.Data(2))
        End Sub

#End Region

#Region "RotateLeft"

        <TestMethod>
        Sub RotateLeftOne()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 24)

            bitList1.RotateLeft(1)

            Assert.AreEqual(24, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(24, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&H4C, bitList1.Data(0))
            Assert.AreEqual(&HF2, bitList1.Data(1))
            Assert.AreEqual(&H81, bitList1.Data(2))
        End Sub

        <TestMethod>
        Sub RotateLeftNibble()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 24)

            bitList1.RotateLeft(4)

            Assert.AreEqual(24, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(24, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&H67, bitList1.Data(0))
            Assert.AreEqual(&H94, bitList1.Data(1))
            Assert.AreEqual(&HA, bitList1.Data(2))
        End Sub

        <TestMethod>
        Sub RotateLeftMultipleBytesOffBoundary()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 20)

            bitList1.RotateLeft(12)

            Assert.AreEqual(20, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(20, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&H94, bitList1.Data(0))
            Assert.AreEqual(&HA6, bitList1.Data(1))
            Assert.AreEqual(&H70, bitList1.Data(2))
        End Sub

        <TestMethod>
        Sub RotateLeftMultipleBytes()
            Dim bytes = New List(Of Byte) From {&HA6, &H79, &H40}
            Dim bitList1 = New BitList(bytes, 24)

            bitList1.RotateLeft(12)

            Assert.AreEqual(24, bitList1.BitCount)
            Assert.AreEqual(0, bitList1.ReadPosition)
            Assert.AreEqual(24, bitList1.WritePosition)
            Assert.AreEqual(0, bitList1.SavedPosition)
            Assert.AreEqual(3, bitList1.Data.Count)
            Assert.AreEqual(&H94, bitList1.Data(0))
            Assert.AreEqual(&HA, bitList1.Data(1))
            Assert.AreEqual(&H67, bitList1.Data(2))
        End Sub

#End Region

    End Class
End Namespace

