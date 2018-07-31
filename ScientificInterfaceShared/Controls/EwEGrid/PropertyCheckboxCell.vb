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

Imports EwECore
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Properties
Imports ScientificInterfaceShared.Style
Imports SourceGrid2

Namespace Controls.EwEGrid

    <CLSCompliant(False)> _
    Public Class PropertyCheckboxCell
        Inherits EwECheckboxCell
        Implements IPropertyCell

        ''' <summary>Connected property.</summary>
        Private m_property As cBooleanProperty = Nothing
        ''' <summary>Flag to detect recursive updates.</summary>
        Private m_bInUpdate As Boolean = False

#Region " Construction and destruction "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor. Note that the indicated property should be a <see cref="cBooleanProperty"/>.
        ''' </summary>
        ''' <param name="pm"><see cref="cPropertyManager">Property manager</see> to extract data from.</param>
        ''' <param name="source">The <see cref="cCoreInputOutputBase">cCoreInputOutputBase</see> data source</param>
        ''' <param name="varName">The <see cref="eVarNameFlags">VarName flag</see> that defines which aspect of the source to acces</param>
        ''' <param name="sourceSec">Optional secundary index to use.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal pm As cPropertyManager, _
                       ByVal source As cCoreInputOutputBase, _
                       ByVal varname As eVarNameFlags, _
                       Optional ByVal sourceSec As cCoreInputOutputBase = Nothing)
            Me.New(CType(pm.GetProperty(source, varname, sourceSec), cBooleanProperty))
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="prop">The property to assign to the cell.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal prop As cBooleanProperty)
            ' Call baseclass constructor
            MyBase.New(False)

            Debug.Assert(TypeOf prop Is cBooleanProperty)

            ' Store the property
            Me.m_property = prop
            ' Fire a change notification
            Me.OnPropertyChanged(prop, cProperty.eChangeFlags.All)
            ' Register property
            AddHandler Me.m_property.PropertyChanged, AddressOf Me.OnPropertyChanged

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Clean up!
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Sub Dispose()

            ' Unregister property
            If (Me.m_property IsNot Nothing) Then
                RemoveHandler Me.m_property.PropertyChanged, AddressOf Me.OnPropertyChanged
                Me.m_property = Nothing
            End If
            MyBase.Dispose()

        End Sub

#End Region ' Construction 

#Region " Data (property)"

        ''' -------------------------------------------------------------------
        ''' <inheritdocs cref="IPropertyCell.GetProperty"/>
        ''' -------------------------------------------------------------------
        Public Function GetProperty() As cProperty _
            Implements IPropertyCell.GetProperty
            Return Me.m_property
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Commonly called in response to end edit.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Sub SetValue(ByVal pos As SourceGrid2.Position, ByVal val As Object)
            ' Intervention
            If (Me.Style And cStyleGuide.eStyleFlags.NotEditable) = cStyleGuide.eStyleFlags.NotEditable Then Return
            ' Continue
            Me.Value = val
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Overridden to access the value maintained by the property
        ''' </summary>
        ''' -------------------------------------------------------------------
        Public Overrides Property Value() As Object
            Get

                ' Does property exist?
                If (Me.m_property IsNot Nothing) Then
                    ' #Yes: return value
                    Return CBool(Me.m_property.GetValue())
                End If
                ' #No: return default
                Return MyBase.Value

            End Get
            Set(ByVal value As Object)

                ' Avoid loops
                If Me.m_bInUpdate Then Return
                Me.m_bInUpdate = True
                Try
                    MyBase.Value = CBool(value)
                Catch ex As Exception

                End Try
                Me.m_bInUpdate = False

                ' Now update property
                Dim bChanged As Boolean = True
                ' Does property exist?
                If (Me.m_property IsNot Nothing) Then
                    ' #Yes: update the property. The property will take care of dispatching any changes
                    bChanged = Me.m_property.SetValue(value, TriState.UseDefault)
                End If
                ' Anything changed?
                If (bChanged) Then
                    ' #Yes: redraw the cell
                    Me.Invalidate()
                End If

            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Allows to set a custom cell <see cref="cStyleGuide.eStyleFlags">style</see>,
        ''' overriding any style in the attached property.
        ''' </summary>
        ''' <remarks>
        ''' <para>Note that this style will not affect the cProperty. Unlike values, which
        ''' can be modified from both core and GUI, Styles are interpreted core status 
        ''' calculations.</para>
        ''' <para>To use a custom Style on a per-cell basis, use <see cref="EwECell.Style">EwECell.Style</see></para>
        ''' <para>To use a custom Style on a system-wide basis for a particular cProperty,
        ''' modify the <see cref="cProperty.SetStyle">Style</see> in the instance of the cProperty.</para>
        ''' </remarks>
        ''' -------------------------------------------------------------------
        Public Overrides Property Style() As cStyleGuide.eStyleFlags
            Get
                Dim s As cStyleGuide.eStyleFlags = MyBase.Style
                If (Me.m_property IsNot Nothing) Then
                    s = Me.m_property.GetStyle()
                End If
                Return s
            End Get
            Set(ByVal s As cStyleGuide.eStyleFlags)
                MyBase.Style = s
            End Set
        End Property

#End Region ' Data (property)

#Region " Updates (property) "

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Property change event handler. Invoked when the property attached 
        ''' to this cell has changed.
        ''' </summary>
        ''' <param name="prop">The <see cref="cProperty">property</see> that changed.</param>
        ''' <param name="changeFlags">Bitwise flag that states what <see cref="cProperty.eChangeFlags">aspect</see>
        ''' of the property has changed.</param>
        ''' -------------------------------------------------------------------
        Protected Overridable Sub OnPropertyChanged(ByVal prop As cProperty, ByVal changeFlags As cProperty.eChangeFlags)

            ' Sanity checks
            Debug.Assert(prop IsNot Nothing, "Invalid event received")

            ' Ignore events for other properties (could be that a derived class is frolicking with strangers, eek)
            If Not Object.Equals(prop, Me.m_property) Then Return

            ' Check style flag changes
            If (changeFlags And cProperty.eChangeFlags.CoreStatus) = cProperty.eChangeFlags.CoreStatus Then
                ' Update read-only state
                Me.DataModel.EnableEdit = ((prop.GetStyle() And cStyleGuide.eStyleFlags.NotEditable) = 0)
            End If

            ' Check for remark changes
            If (changeFlags And cProperty.eChangeFlags.Remarks) = cProperty.eChangeFlags.Remarks Then
                Me.ToolTipText = prop.GetRemark()
            End If

            ' Redraw the cell
            Me.Invalidate()

        End Sub

#End Region ' Updates (property)

    End Class

End Namespace
