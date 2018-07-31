' ===============================================================================
' This file is part of Ecopath with Ecosim (EwE)
'
' EwE is free software: you can redistribute it and/or modify it under the terms
' of the GNU General Public License version 2 as published by the Free Software 
' Foundation.
'
' EwE is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
' without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
' PURPOSE. See the GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http://www.gnu.org/licenses/gpl-2.0.html>. 
'
' The Cefas MSE plug-in was developed by the Centre for Environment, Fisheries and 
' Aquaculture Science (Cefas). 
'
' EwE copyright 1991- :
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
' Cefas MSE plug-in copyright: 
'    2013- Cefas, Lowestoft, UK.
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports EwECore
Imports EwECore.MSE
Imports EwEUtils.Core
Imports SharedResources = ScientificInterfaceShared.My.Resources
Imports SourceGrid2
Imports SourceGrid2.Cells
Imports ScientificInterfaceShared.Controls.EwEGrid

Imports ScientificInterfaceShared.Style


#End Region ' Imports



''' ===========================================================================
''' <summary>
''' Grid to allow species quota interaction.
''' </summary>
''' ===========================================================================
<CLSCompliant(False)> _
Public Class gridErrorCVs
    Inherits EwEGrid

    Private m_Assessment As cStockAssessmentModel

    Private m_CurSelectedDataType As frmEditAssessmentError.eErrorDataType

#Region " Internal defs "

    Private Enum eColumnTypes As Integer
        Index = 0
        Name
        ErrorCol
    End Enum

#End Region ' Internal defs

    Public Event onEdited()

#Region " Constructor "

    Public Sub New()
        MyBase.new()

        m_CurSelectedDataType = frmEditAssessmentError.eErrorDataType.GroupObervationError

    End Sub

    Public Sub Init(ByVal StockAssessmentModel As cStockAssessmentModel)
        m_Assessment = StockAssessmentModel
    End Sub

#End Region ' Constructor

#Region " Public interfaces "


    Public Property ErrorDataType As frmEditAssessmentError.eErrorDataType
        Get
            Return Me.m_CurSelectedDataType
        End Get
        Set(value As frmEditAssessmentError.eErrorDataType)

            Me.m_CurSelectedDataType = value
            If (Me.UIContext Is Nothing) Then Return

            Me.InitStyle()
            Me.FillData()

        End Set

    End Property

#End Region ' Public interfaces

#Region " Overrides "

    Protected Overrides Sub InitStyle()
        MyBase.InitStyle()

        ' ToDo: localize this

        Dim iNumCols As Integer = [Enum].GetValues(GetType(eColumnTypes)).Length

        Me.Redim(1, iNumCols)

        Dim colName As String = ""
        Dim errorCaption As String = ""
        Select Case Me.m_CurSelectedDataType
            Case frmEditAssessmentError.eErrorDataType.FleetImplementationError
                colName = SharedResources.HEADER_FLEETNAME
                errorCaption = "Implementation Error"
            Case frmEditAssessmentError.eErrorDataType.GroupObervationError
                colName = SharedResources.HEADER_GROUPNAME
                errorCaption = "Observation Error"
        End Select

        Me(0, eColumnTypes.Index) = New EwEColumnHeaderCell("")
        Me(0, eColumnTypes.Name) = New EwEColumnHeaderCell(colName)
        Me(0, eColumnTypes.ErrorCol) = New EwEColumnHeaderCell(errorCaption)
        Me.FixedColumns = 2
        Me.FixedColumnWidths = False

    End Sub

    Protected Overrides Sub FillData()

        If Me.m_Assessment Is Nothing Then Return

        Select Case Me.m_CurSelectedDataType

            Case frmEditAssessmentError.eErrorDataType.FleetImplementationError
                FillFleetData()
            Case frmEditAssessmentError.eErrorDataType.GroupObervationError
                FillGroupData()

        End Select

    End Sub


    Private Sub FillFleetData()
        Dim Fleet As cStockAssessmentFleetParameters
        Dim Cell As ICell
        Dim irow As Integer

        'For each Fleet
        For iFlt As Integer = 1 To Core.nFleets

            'Fleet parameter object for this row/fleet
            Fleet = Me.m_Assessment.FleetParameter(iFlt)

            irow = Me.AddRow()

            Me(iFlt, eColumnTypes.Index) = New EwERowHeaderCell(CStr(iFlt))
            Cell = New EwECell(Fleet.Name, GetType(String), cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Names)
            Me(irow, eColumnTypes.Name) = Cell

            Cell = New EwECell(Fleet.cvImpError, GetType(Single))
            Cell.Behaviors.Add(Me.EwEEditHandler)
            Me(irow, eColumnTypes.ErrorCol) = Cell

            Me.Rows(iFlt).Tag = Fleet

        Next iFlt
    End Sub


    Private Sub FillGroupData()
        Dim Group As cStockAssessmentParameters
        Dim Cell As ICell
        Dim irow As Integer

        'For each group
        For iGrp As Integer = 1 To Core.nLivingGroups

            'Group parameter object for this row/group
            Group = Me.m_Assessment.Parameter(iGrp)

            irow = Me.AddRow()

            Me(iGrp, eColumnTypes.Index) = New EwERowHeaderCell(CStr(iGrp))
            Cell = New EwECell(Group.Name, GetType(String), cStyleGuide.eStyleFlags.NotEditable Or cStyleGuide.eStyleFlags.Names)
            Me(irow, eColumnTypes.Name) = Cell

            Cell = New EwECell(Group.CVObservationError, GetType(Single))
            Cell.Behaviors.Add(Me.EwEEditHandler)
            Me(irow, eColumnTypes.ErrorCol) = Cell

            Me.Rows(iGrp).Tag = Group

        Next iGrp
    End Sub

    Protected Overrides Sub FinishStyle()
        MyBase.FinishStyle()
    End Sub

    Public Overrides ReadOnly Property MessageSource() As eCoreComponentType
        Get
            Return eCoreComponentType.MSE
        End Get
    End Property

    Protected Overrides Function OnCellEdited(p As SourceGrid2.Position, cell As SourceGrid2.Cells.ICellVirtual) As Boolean
        Dim bEdited As Boolean
        If Rows(p.Row).Tag Is Nothing Then
            'No Group in this row
            Return True
        End If

        Try

            'Changing the value of the parameter 
            'forces the model to init to the new value
            'and redraws the interface
            'so only update if the value is actually new 
            Dim newValue As Single = CSng(cell.GetValue(p))

            Select Case p.Column

                'Only the Error column is editable
                Case eColumnTypes.ErrorCol
                    'Check the currently selected data type for Group or Fleet Error
                    Select Case Me.m_CurSelectedDataType

                        'Fleet implementation error
                        Case frmEditAssessmentError.eErrorDataType.FleetImplementationError
                            Dim param As cStockAssessmentFleetParameters = DirectCast(Rows(p.Row).Tag, cStockAssessmentFleetParameters)
                            If param.cvImpError <> newValue Then
                                param.cvImpError = newValue
                                bEdited = True
                            End If

                            'Group observation error
                        Case frmEditAssessmentError.eErrorDataType.GroupObervationError

                            Dim param As cStockAssessmentParameters = DirectCast(Rows(p.Row).Tag, cStockAssessmentParameters)
                            If param.CVObservationError <> newValue Then
                                param.CVObservationError = newValue
                                bEdited = True
                            End If

                    End Select

            End Select

        Catch ex As Exception
            Debug.Assert(False, Me.ToString + ".OnCellEdited() Exception: " + ex.Message)
        End Try

        'If bEdited Then
        Try
                RaiseEvent onEdited()
            Catch ex As Exception

            End Try
        'End If

        Return MyBase.OnCellEdited(p, cell)

    End Function



#End Region ' Overrides

End Class


