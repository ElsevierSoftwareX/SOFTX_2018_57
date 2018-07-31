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
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Properties

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Implements an expression as a binary operation, i.e. an
    ''' arithmetical computation on two numerals.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Class cBinaryOperation
        : Inherits cExpression

        ''' ---------------------------------------------------------------
        ''' <summary>Supported arithmetical operations</summary>
        ''' ---------------------------------------------------------------
        Public Enum eOperatorType
            ''' <summary>Adds operand 1 to operand 2</summary>
            Add
            ''' <summary>Subtracts operand 2 from operand 1</summary>
            Subtract
            ''' <summary>Multiplies operand 1 with operand 2</summary>
            Multiply
            ''' <summary>Divides operand 1 by operand 2</summary>
            Divide
            ''' <summary>Raises operand 1 to the power of operand 2</summary>
            Pow
            ''' <summary>Returns the larger of 2 operands</summary>
            Max
            ''' <summary>Returns the lower of 2 operands</summary>
            Min
        End Enum

        ''' <summary>Operator to perform</summary>
        Private m_nOperator As eOperatorType = 0
        ''' <summary>First operand</summary>
        Private WithEvents m_operand1 As cExpression = Nothing
        ''' <summary>Second operand</summary>
        Private WithEvents m_operand2 As cExpression = Nothing
        ''' <summary>Cached value</summary>
        Private m_sValue As Single = 0.0
        ''' <summary>Cached style</summary>
        Private m_style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new cBinaryOperation. 
        ''' </summary>
        ''' <param name="nOperator"><see cref="eOperatorType">Operator</see> to perform.</param>
        ''' <param name="operand1">First operand (left side of operator).</param>
        ''' <param name="operand2">Second operand (right side of operator).</param>
        ''' <remarks>For supported operand types, see <see cref="cFormulaProperty.GetExpression">cFormulaProperty.GetExpression</see>.</remarks>
        ''' ---------------------------------------------------------------
        Public Sub New(ByVal nOperator As eOperatorType, ByVal operand1 As Object, ByVal operand2 As Object)
            Me.m_nOperator = nOperator
            Me.m_operand1 = cFormulaProperty.GetExpression(operand1)
            Me.m_operand2 = cFormulaProperty.GetExpression(operand2)
            Me.m_sValue = Me.CalcValue()
            Me.m_style = Me.CalcStyle()
            ' Start listening for operand changes
            AddHandler Me.m_operand1.OnValueChanged, AddressOf OnOperandValueChanged
            AddHandler Me.m_operand2.OnValueChanged, AddressOf OnOperandValueChanged
        End Sub

        Protected Overrides Sub Dispose(ByVal bDisposing As Boolean)
            ' Stop listening for operand changes
            RemoveHandler Me.m_operand1.OnValueChanged, AddressOf OnOperandValueChanged
            RemoveHandler Me.m_operand2.OnValueChanged, AddressOf OnOperandValueChanged
            ' Clean up
            Me.m_operand1.Dispose()
            Me.m_operand2.Dispose()
            Me.m_operand1 = Nothing
            Me.m_operand2 = Nothing
        End Sub

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Returns the outcome of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Public Overrides Function GetValue() As Single
            Return Me.m_sValue
        End Function

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Returns the <see cref="cStyleGuide.eStyleFlags">style</see>
        ''' of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Public Overrides Function GetStyle() As cStyleGuide.eStyleFlags
            Return Me.m_style
        End Function

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Event handler, responds to operand change events by recalculating
        ''' the outcome of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Private Sub OnOperandValueChanged(ByVal exp As cExpression)
            ' Calc new value and style
            Dim sVal As Single = Me.CalcValue()
            Dim style As cStyleGuide.eStyleFlags = Me.CalcStyle()
            ' Changes?
            If ((sVal <> Me.m_sValue) Or (Me.m_style <> style)) Then
                ' #Yes: set new value and style
                Me.m_sValue = sVal
                Me.m_style = style

                ' Broadcast change notification
                FireChangeNotification()
            End If
        End Sub

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Recalculate the outcome of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Private Function CalcValue() As Single
            Dim s1 As Single = Me.m_operand1.GetValue()
            Dim s2 As Single = Me.m_operand2.GetValue()
            Dim s As Single = 0

            Select Case Me.m_nOperator
                Case eOperatorType.Add
                    s = s1 + s2
                Case eOperatorType.Subtract
                    s = s1 - s2
                Case eOperatorType.Divide
                    s = s1 / s2
                Case eOperatorType.Multiply
                    s = s1 * s2
                Case eOperatorType.Pow
                    s = CSng(Math.Pow(s1, s2))
                Case eOperatorType.Max
                    s = CSng(Math.Max(1, s2))
                Case eOperatorType.Min
                    s = CSng(Math.Min(s1, s2))
                Case Else
                    Debug.Assert(False, String.Format("Operator {0} not implemented", Me.m_nOperator))
            End Select

            Return s
        End Function

        ''' ---------------------------------------------------------------
        ''' <summary>
        ''' Recalculate the <see cref="cStyleGuide.eStyleFlags">style</see>
        ''' of the operation.
        ''' </summary>
        ''' ---------------------------------------------------------------
        Private Function CalcStyle() As cStyleGuide.eStyleFlags
            Return (Me.m_operand1.GetStyle() Or Me.m_operand2.GetStyle())
        End Function

    End Class

End Namespace
