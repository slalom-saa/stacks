﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Slalom.Stacks.Messaging;
using Slalom.Stacks.Messaging.Registry;


namespace Slalom.Stacks.Documentation.Model
{
    public class EndPointElement
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string Timeout { get; set; }

        public int Version { get; set; }

        public List<ParameterElement> Parameters { get; set; } = new List<ParameterElement>();

        public List<RuleElement> Rules { get; set; } = new List<RuleElement>();

        public List<TestElement> Tests { get; set; } = new List<TestElement>();

        public List<DependencyElement> Dependencies { get; set; } = new List<DependencyElement>();

        public EndPointElement(string name, EndPointMetaData endPoint, IEnumerable<Type> tests)
        {
            this.Name = name;
            this.Path = endPoint.Path;
            this.Timeout = endPoint.Timeout.ToString();
            this.Version = endPoint.Version;

            foreach (var property in endPoint.RequestProperties)
            {
                this.Parameters.Add(new ParameterElement(property));
                if (property.Validation != null)
                {
                    this.Rules.Add(new RuleElement(property));
                }
            }

            foreach (var rule in endPoint.Rules)
            {
                this.Rules.Add(new RuleElement(rule));
            }

            foreach (var test in tests)
            {
                foreach (var method in test.GetTypeInfo().GetMethods())
                {
                    if (method.DeclaringType == test)
                    {
                        this.Tests.Add(new TestElement(method));
                    }
                }
            }
        }

        public EndPointElement(INamedTypeSymbol item, List<INamedTypeSymbol> siblings)
        {
            this.Name = item.Name;
            var meta = item.GetAttributes().FirstOrDefault(e => e.AttributeClass.Name == "EndPointAttribute");
            if (meta != null)
            {
                this.Path = meta.ConstructorArguments[0].Value.ToString();
                this.GetValue(meta, "Name", e => this.Name = e.ToString());
                this.GetValue(meta, "Version", e => this.Version = (int)e);
                this.GetValue(meta, "Timeout", e => this.Timeout = e.ToString());
            }

            var request = item.BaseType.TypeArguments.FirstOrDefault();
            if (request != null)
            {
                var members = request.GetMembers().OfType<IPropertySymbol>();
                foreach (var member in members.Where(e => e.Kind == SymbolKind.Property))
                {
                    this.Parameters.Add(new ParameterElement(member));

                    var attribute = member.GetAttributes().FirstOrDefault(e => e.AttributeClass.Name == "NotNullAttribute");
                    if (attribute != null)
                    {
                        this.Rules.Add(new RuleElement("Input", attribute.ConstructorArguments[0].Value.ToString()));
                    }
                }

                var rules = siblings.Where(e => e.BaseType?.Name == "SecurityRule" && e.BaseType.TypeArguments.FirstOrDefault().Name == request.Name);
                foreach (var rule in rules)
                {
                    this.Rules.Add(new RuleElement("Security", new Comments(rule.GetDocumentationCommentXml())));
                }

                rules = siblings.Where(e => e.BaseType?.Name == "BusinessRule" && e.BaseType.TypeArguments.FirstOrDefault().Name == request.Name);
                foreach (var rule in rules)
                {
                    this.Rules.Add(new RuleElement("Business", new Comments(rule.GetDocumentationCommentXml())));
                }

                var tests = siblings.Where(e => e.GetAttributes().Any(a => a.AttributeClass.Name == "TestSubjectAttribute" && ((INamedTypeSymbol) a.ConstructorArguments.First().Value).Name == item.Name));
                foreach (var test in tests)
                {
                    foreach (var method in test.GetMembers().OfType<IMethodSymbol>())
                    {
                        if (method.ContainingType.Name == test.Name && method.Name != ".ctor")
                        {
                            this.Tests.Add(new TestElement(method));
                        }
                    }
                }
            }

           
        }

        private void GetValue(AttributeData meta, string name, Action<object> setter)
        {
            if (meta.NamedArguments.Any(e => e.Key == name))
            {
                setter(meta.NamedArguments.First(e => e.Key == name).Value.Value);
            }
        }
    }
}
