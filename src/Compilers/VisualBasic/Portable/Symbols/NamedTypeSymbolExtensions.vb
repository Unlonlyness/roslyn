﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System
Imports System.Collections.Immutable
Imports System.Runtime.CompilerServices
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic.Symbols
    Friend Module NamedTypeSymbolExtensions

        ''' <summary>
        ''' Safe to call on a null reference.
        ''' </summary>
        <Extension()>
        Friend Function IsOrInGenericType(toCheck As NamedTypeSymbol) As Boolean
            ' TODO: Should we do a quick check for [Not IsDefinition()]?

            While toCheck IsNot Nothing
                If toCheck.Arity > 0 Then
                    Return True
                End If

                toCheck = toCheck.ContainingType
            End While
            Return False
        End Function

        <Extension()>
        Friend Function FindMember(container As NamedTypeSymbol, symbolName As String, kind As SymbolKind, nameSpan As TextSpan, tree As SyntaxTree) As Symbol
            ' Search all symbol declarations for the right one. We do a quick lookup by name, then
            ' linear search on all symbols with that name.
            For Each child In container.GetMembers(symbolName)
                If child.Kind = kind Then
                    For Each methodLoc In child.Locations
                        If methodLoc.IsInSource AndAlso methodLoc.SourceTree Is tree AndAlso methodLoc.SourceSpan = nameSpan Then
                            Return child
                        End If
                    Next

                    ' For partial methods also check partial implementation
                    If kind = SymbolKind.Method Then
                        Dim partialImpl = DirectCast(child, MethodSymbol).PartialImplementationPart
                        If partialImpl IsNot Nothing Then
                            For Each methodLoc In partialImpl.Locations
                                If methodLoc.IsInSource AndAlso methodLoc.SourceTree Is tree AndAlso methodLoc.SourceSpan = nameSpan Then
                                    Return partialImpl
                                End If
                            Next
                        End If
                    End If
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Given a name, find a member field or property (ignoring all other members) in a type.
        ''' </summary>
        <Extension()>
        Friend Function FindFieldOrProperty(container As NamedTypeSymbol, symbolName As String, nameSpan As TextSpan, tree As SyntaxTree) As Symbol
            ' Search all symbol declarations for the right one. We do a quick lookup by name, then
            ' linear search on all symbols with that name.
            For Each child In container.GetMembers(symbolName)
                If child.Kind = SymbolKind.Field OrElse child.Kind = SymbolKind.Property Then
                    For Each methodLoc In child.Locations
                        If methodLoc.IsInSource AndAlso
                           methodLoc.SourceTree Is tree AndAlso
                           methodLoc.SourceSpan = nameSpan Then
                            Return child
                        End If
                    Next
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Given a possibly constructed/specialized generic type, create a symbol
        ''' representing an unbound generic type for its definition.
        ''' </summary>
        <Extension()>
        Public Function AsUnboundGenericType(this As NamedTypeSymbol) As NamedTypeSymbol
            Return UnboundGenericType.Create(this)
        End Function


        <Extension()>
        Friend Function HasVariance(this As NamedTypeSymbol) As Boolean
            Dim current As NamedTypeSymbol = this

            Do
                If current.TypeParameters.HaveVariance() Then
                    Return True
                End If

                current = current.ContainingType
            Loop While current IsNot Nothing

            Return False
        End Function

        <Extension()>
        Friend Function HaveVariance(this As ImmutableArray(Of TypeParameterSymbol)) As Boolean
            For Each tp In this
                Select Case tp.Variance
                    Case VarianceKind.In, VarianceKind.Out
                        Return True
                End Select
            Next

            Return False
        End Function

    End Module
End Namespace
