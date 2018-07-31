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
' Copyright 1991- 
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and
'    Ecopath International Initiative, Barcelona, Spain
' ===============================================================================
'

#Region " Imports "

Option Strict On

Imports EwEUtils.Core
Imports ScientificInterfaceShared.Style
Imports System.Globalization
Imports EwEUtils.Utilities

#End Region ' Imports

Namespace Controls

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Combo box that allows the user to select a monetary unit.
    ''' </summary>
    ''' ---------------------------------------------------------------------------
    Public Class cMonetaryUnitComboBox
        Implements IUIElement

#Region " Helper classes "

        Private Class MonetaryUnitItem

            Private m_strISOSymbol As String = ""
            Private m_strDescription As String

            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="m_strISOSymbol"></param>
            ''' <param name="strDescription"></param>
            ''' <remarks></remarks>
            Public Sub New(ByVal m_strISOSymbol As String, ByVal strDescription As String)
                Me.m_strISOSymbol = m_strISOSymbol
                Me.m_strDescription = strDescription
            End Sub

            Public ReadOnly Property ISOSymbol() As String
                Get
                    Return Me.m_strISOSymbol
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return String.Format(My.Resources.GENERIC_LABEL_DOUBLE, Me.m_strISOSymbol, Me.m_strDescription)
            End Function

        End Class

#End Region ' Helper classes

#Region " Private vars "

        Private m_uic As cUIContext = Nothing

#End Region ' Private vars

        Public Sub New()
            Me.InitializeComponent()
            Me.DropDownStyle = ComboBoxStyle.DropDownList
        End Sub

        Public Property UIContext() As cUIContext _
            Implements IUIElement.UIContext
            Get
                Return Me.m_uic
            End Get
            Set(ByVal value As cUIContext)
                Me.m_uic = value
                Me.Populate()
            End Set
        End Property

        Private Sub Populate()

            Dim fmt As New cMonetaryTypeFormatter()

            If Me.m_uic Is Nothing Then Return

            Me.SuspendLayout()

            For Each ci As CultureInfo In CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                Try
                    Dim ri As New RegionInfo(ci.LCID)
                    Dim strAbbr As String = fmt.GetDescriptor(ri, eDescriptorTypes.Abbreviation)
                    Dim strName As String = fmt.GetDescriptor(ri, eDescriptorTypes.Name)

                    If Me.GetUnitIndex(strAbbr) = -1 Then
                        Me.Items.Add(New MonetaryUnitItem(strAbbr, strName))
                    End If
                Catch ex As Exception
                    ' Swallow this
                End Try
            Next

            Me.Sorted = True
            Me.ResumeLayout()

        End Sub

        Public Property Unit() As String
            Get
                If TypeOf Me.SelectedItem Is MonetaryUnitItem Then
                    Return DirectCast(Me.SelectedItem, MonetaryUnitItem).ISOSymbol
                Else
                    Return "EUR"
                End If
            End Get
            Set(ByVal value As String)
                Me.SelectedIndex = GetUnitIndex(value)
            End Set
        End Property

        Public Function GetUnitIndex(ByVal strUnit As String) As Integer
            For iItem As Integer = 0 To Me.Items.Count - 1
                If TypeOf Me.Items(iItem) Is MonetaryUnitItem Then
                    If DirectCast(Me.Items(iItem), MonetaryUnitItem).ISOSymbol = strUnit Then
                        Return iItem
                    End If
                End If
            Next
            Return -1
        End Function
    End Class

End Namespace
