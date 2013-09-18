﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;

namespace ReSharperFixieRunner.UnitTestProvider.Elements
{
    public class FixieTestClassElement : FixieBaseElement
    {
        private readonly DeclaredElementProvider declaredElementProvider;
        private readonly IClrTypeName typeName;
        private readonly string assemblyLocation;
        
        public FixieTestClassElement(FixieTestProvider provider, ProjectModelElementEnvoy projectModelElementEnvoy, DeclaredElementProvider declaredElementProvider, string id, IClrTypeName typeName, string assemblyLocation)
            : base(provider, null, id, projectModelElementEnvoy)
        {
            this.declaredElementProvider = declaredElementProvider;
            this.typeName = typeName;
            this.assemblyLocation = assemblyLocation;

            ShortName = string.Join("+", typeName.TypeNames.Select(FormatTypeName).ToArray());
        }

        public override bool Equals(IUnitTestElement other)
        {
            return Equals(other as FixieTestClassElement);
        }

        public bool Equals(FixieTestClassElement other)
        {
            if (other == null)
                return false;

            return Equals(Id, other.Id) &&
                   Equals(typeName, other.typeName) &&
                   Equals(AssemblyLocation, other.AssemblyLocation);
        }

        public override string GetPresentation(IUnitTestElement parent = null)
        {
            return ShortName;
        }

        public override UnitTestNamespace GetNamespace()
        {
            return new UnitTestNamespace(typeName.GetNamespaceName());
        }

        public override UnitTestElementDisposition GetDisposition()
        {
            var element = GetDeclaredElement();
            if (element == null || !element.IsValid())
                return UnitTestElementDisposition.InvalidDisposition;

            var locations = from declaration in element.GetDeclarations()
                            let file = declaration.GetContainingFile()
                            where file != null
                            select new UnitTestElementLocation(file.GetSourceFile().ToProjectFile(),
                                                               declaration.GetNameDocumentRange().TextRange,
                                                               declaration.GetDocumentRange().TextRange);
            return new UnitTestElementDisposition(locations, this);
        }

        public override IDeclaredElement GetDeclaredElement()
        {
            return declaredElementProvider.GetDeclaredElement(GetProject(), typeName);
        }

        public override IEnumerable<IProjectFile> GetProjectFiles()
        {
            var declaredElement = GetDeclaredElement();
            if (declaredElement == null)
                return EmptyArray<IProjectFile>.Instance;

            return from sourceFile in declaredElement.GetSourceFiles()
                   select sourceFile.ToProjectFile();
        }

        public override IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestLaunch launch)
        {
            // TODO: Populate with useful tasks when we have them
            return new List<UnitTestTask>();
        }

        public override string Kind
        {
            get { return "Fixie Test Class"; }
        }
 
        public string AssemblyLocation { get { return assemblyLocation; } }

        private static string FormatTypeName(TypeNameAndTypeParameterNumber typeName)
        {
            return typeName.TypeName + (typeName.TypeParametersNumber > 0 ? string.Format("`{0}", typeName.TypeParametersNumber) : string.Empty);
        }
    }
}