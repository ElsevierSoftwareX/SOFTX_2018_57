#Region " Imports "

Option Strict On
Option Explicit On

Imports EwECore
Imports SourceGrid2.Cells.Real
Imports EwEUtils.Core
Imports EwECore.MSE

#End Region


''' <summary>
''' Performance results grid from EwE5 
''' </summary>
''' <remarks>No longer used by EwE6</remarks>
<CLSCompliant(False)> _
Public Class gridPerformanceResults
    : Inherits EwEGrid

    Public Sub New()
    End Sub

    Protected Overrides Sub InitStyle()

        ' ToDo: localize this method

        MyBase.InitStyle()
        Me.Redim(1, 3)
        Me(0, 0) = New EwEColumnHeaderCell("Criteria")
        Me(0, 1) = New EwEColumnHeaderCell("'Open Loop' simulation w/o assessment")
        Me(0, 2) = New EwEColumnHeaderCell("Means achieved over 'Close Loop' simulation trials")

    End Sub

    Protected Overrides Sub FillData()

        Try
            ' ToDo: localize this method

            Dim mse As cMSEManager = Me.Core.MSEManager
            If mse Is Nothing Then Exit Sub
            Dim row As Integer = 1

            Me.Rows.Insert(row)
            Me(row, 0) = New EwERowHeaderCell("Net economic value")
            Me(row, 1) = New PropertyCell(Me.PropertyManager, mse.Output, eVarNameFlags.MSEEconomicValue)
            Me(row, 2) = New PropertyCell(Me.PropertyManager, mse.Output, eVarNameFlags.MSEMeanEconomicValue)

            row += 1
            Me.Rows.Insert(row)
            Me(row, 0) = New EwERowHeaderCell("Social(employment) value")
            Me(row, 1) = New PropertyCell(Me.PropertyManager, mse.Output, eVarNameFlags.MSEEmployValue)
            Me(row, 2) = New PropertyCell(Me.PropertyManager, mse.Output, eVarNameFlags.MSEMeanEmployValue)

            row += 1
            Me.Rows.Insert(row)
            Me(row, 0) = New EwERowHeaderCell("Mandated rebuilding")
            Me(row, 1) = New PropertyCell(Me.PropertyManager, mse.Output, eVarNameFlags.MSEMandatedValue)
            Me(row, 2) = New PropertyCell(Me.PropertyManager, mse.Output, eVarNameFlags.MSEMeanMandatedValue)

            row += 1
            Me.Rows.Insert(row)
            Me(row, 0) = New EwERowHeaderCell("Ecosytem structure")
            Me(row, 1) = New PropertyCell(Me.PropertyManager, mse.Output, eVarNameFlags.MSEEcologicalValue)
            Me(row, 2) = New PropertyCell(Me.PropertyManager, mse.Output, eVarNameFlags.MSEMeanEcologicalValue)

            row += 1
            Me.Rows.Insert(row)
            Me(row, 0) = New EwERowHeaderCell("Overall value")
            Me(row, 1) = New PropertyCell(Me.PropertyManager, mse.Output, eVarNameFlags.MSEBestTotalValue)
            Me(row, 2) = New PropertyCell(Me.PropertyManager, mse.Output, eVarNameFlags.MSEWeightedTotalValue)

        Catch ex As Exception
            Debug.Assert(False)
        End Try



    End Sub

End Class
