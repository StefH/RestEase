﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using RestEase;
using RestEase.Implementation;
using Xunit;
using Xunit.Abstractions;

namespace RestEaseUnitTests.ImplementationFactoryTests
{
    public class InterfaceInheritanceTests : ImplementationFactoryTestsBase
    {
        public interface IPropertyChild
        {
            [Header("X-Api-Token")]
            string ApiToken { get; set; }
        }

        public interface IPropertyParent : IPropertyChild
        {
            [Header("X-Api-Username")]
            string ApiUsername { get; set; }
        }

        public interface IMethodChild
        {
            [Get("/foo")]
            Task<string> GetFoo();
        }

        public interface IMethodParent : IMethodChild
        {
            [Get("/bar")]
            Task<string> GetBar();
        }

        [Header("X-Foo", "Bar")]
        public interface IChildWithInterfaceHeader
        {
        }

        [Header("X-Foo", "Baz")]
        public interface IParentWithInterfaceHeader : IChildWithInterfaceHeader
        {
            [Get("/foo")]
            Task FooAsync();
        }

        [AllowAnyStatusCode]
        public interface IChildWithAllowAnyStatusCode
        {
        }

        public interface IParentWithAllowAnyStatusCode : IChildWithAllowAnyStatusCode
        {
        }

        public interface IChildWithInvalidHeaderProperty
        {
            [Header("X-Foo:")]
            string Foo { get; set; }
        }

        public interface IParentWithInvalidHeaderProperty : IChildWithInvalidHeaderProperty
        {
        }

        public interface IChildWithEvent
        {
            event EventHandler Foo;
        }

        public interface IParentWithEvent : IChildWithEvent
        {
        }

        public InterfaceInheritanceTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void ImplementsPropertiesFromChild()
        {
            // Does not throw
            this.VerifyDiagnostics<IPropertyParent>();
        }

        [Fact]
        public void ImplementsMethodsFromChild()
        {
            // Does not throw
            this.VerifyDiagnostics<IMethodParent>();
        }

        [Fact]
        public void CombinesHeadersFromChildInterfaces()
        {
            var requestInfo = this.Request<IParentWithInterfaceHeader>(x => x.FooAsync());
            var classHeaders = requestInfo.ClassHeaders.ToList();

            Assert.Equal("X-Foo", classHeaders[0].Key);
            Assert.Equal("Baz", classHeaders[0].Value);

            Assert.Equal("X-Foo", classHeaders[1].Key);
            Assert.Equal("Bar", classHeaders[1].Value);
        }

        [Fact]
        public void DoesNotAllowAllowAnyStatusCodeOnChildInterfaces()
        {
            this.VerifyDiagnostics<IParentWithAllowAnyStatusCode>(
                // (-4,10): Error REST014: Parent interface (of tyoe 'IParentWithAllowAnyStatusCode') may not have an [AllowAnyStatusCode] attribute
                // AllowAnyStatusCode
                Diagnostic(DiagnosticCode.AllowAnyStatusCodeAttributeNotAllowedOnParentInterface, "AllowAnyStatusCode").WithLocation(-4, 10)
            );
        }

        [Fact]
        public void ValidatesHeadersOnChildProperties()
        {
            this.VerifyDiagnostics<IParentWithInvalidHeaderProperty>(
                // (-3,14): Error REST010: Header attribute name 'X-Foo:' must not contain a colon
                // Header("X-Foo:")
                Diagnostic(DiagnosticCode.HeaderMustNotHaveColonInName, @"Header(""X-Foo:"")").WithLocation(-3, 14)
            );
        }

        [Fact]
        public void ValidatesEventsInChildInterfaces()
        {
            this.VerifyDiagnostics<IParentWithEvent>(
                // (-2,32): Error REST015: Intarfaces must not have any events
                // Foo,
                Diagnostic(DiagnosticCode.EventsNotAllowed, "Foo").WithLocation(-2, 32)
            );
        }
    }
}
