using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace pyr.Union.SourceGenerators;

internal class UnionTypeReceiver : ISyntaxReceiver
{
    public List<TypeDeclarationSyntax> Candidates { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not TypeDeclarationSyntax typeDecl) return;
        if (IsUnionTypeCandidate(typeDecl)) Candidates.Add(typeDecl);
    }

    private static bool IsUnionTypeCandidate(TypeDeclarationSyntax typeDecl)
    {
        return typeDecl.AttributeLists.SelectMany(x => x.Attributes).Any();
    }
}
