using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Validation;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using T = System.Threading.Tasks;

namespace Hl7.Fhir.Specification.Tests.Validation
{
    [Trait("Category", "Validation")]
    public class ChargeItemDefinitionValidationTests : IClassFixture<ValidationFixture>
    {
        private readonly IAsyncResourceResolver _asyncSource;
        private readonly IResourceResolver _source;
        private readonly Validator _validator;

        public ChargeItemDefinitionValidationTests(ValidationFixture fixture)
        {
            _source = fixture.Resolver;
            _asyncSource = fixture.AsyncResolver;
            _validator = fixture.Validator;
        }

        /// <summary>
        /// Test for ChargeItemDefinition resource validation with minimal required fields.
        /// </summary>
        [Fact]
        public void ValidateChargeItemDefinitionMinimal()
        {
            var definition = new ChargeItemDefinition
            {
                Id = "example",
                Url = "http://example.org/fhir/ChargeItemDefinition/example",
                Status = PublicationStatus.Active
            };

            var report = _validator.Validate(definition);
            report.Success.Should().BeTrue($"ChargeItemDefinition with required fields should be valid. Issues: {report}");
            report.Errors.Should().Be(0);
        }

        /// <summary>
        /// SPEC ERROR TEST: ChargeItemDefinition constraint cid-0 references a "name" field
        /// that doesn't exist in the resource model.
        ///
        /// According to FHIR R4 Specification:
        /// - Constraint ID: cid-0
        /// - Description: "Name should be usable as an identifier for the module by machine processing applications such as code generation"
        /// - FHIRPath Expression: name.matches('[A-Z]([A-Za-z0-9_]){0,254}')
        /// - Issue: The ChargeItemDefinition resource does NOT have a "name" field
        ///
        /// Related JIRA Tickets:
        /// - FHIR-34622: ChargeItemDefinition is missing canonical properties
        /// - FHIR-25527: ChargeItemDefinition should be canonical-style resource
        ///
        /// Resolution: ChargeItemDefinition should have been defined as a canonical-style
        /// resource with all canonical base properties, including "name".
        /// </summary>
        [Fact]
        public void ChargeItemDefinition_MissingNameProperty_ConstraintCID0Unenforceable()
        {
            // Arrange: Create a ChargeItemDefinition with all available properties
            var definition = new ChargeItemDefinition
            {
                Id = "constraint-validation-test",
                Url = "http://example.org/fhir/ChargeItemDefinition/test-definition",
                Version = "1.0.0",
                Status = PublicationStatus.Active,
                Title = "Test Charge Item Definition",
                Description = "A test definition for validating constraint cid-0",
                ExperimentalElement = new FhirBoolean(false),
                Date = "2023-01-15",
                PublisherElement = new FhirString("Test Publisher"),
            };

            // Verify the ChargeItemDefinition object has no "name" property
            var nameProperty = definition.GetType().GetProperty("Name",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.IgnoreCase);

            // Act & Assert
            nameProperty.Should().BeNull(
                because: "ChargeItemDefinition should not have a 'Name' property according to current R4 model, " +
                "but constraint cid-0 validates: name.matches('[A-Z]([A-Za-z0-9_]){0,254}'). " +
                "This is a spec error - see JIRA FHIR-34622 and FHIR-25527.");

            // The constraint cid-0 cannot be validated because the field doesn't exist
            var report = _validator.Validate(definition);
            // Note: The validator may not enforce cid-0 since the field is missing
            report.Success.Should().BeTrue(
                because: "ChargeItemDefinition validates without 'name' field because the field does not exist in the model. " +
                "This confirms the constraint cid-0 cannot be enforced as written.");
        }

        /// <summary>
        /// Demonstrates that ChargeItemDefinition lacks canonical resource properties.
        ///
        /// The resource is defined with url, status, and other properties, but is missing
        /// properties that canonical resources should have (like name, title, description, etc).
        ///
        /// This test documents that ChargeItemDefinition is defined inconsistently with
        /// other canonical resources in FHIR R4.
        /// </summary>
        [Fact]
        public void ChargeItemDefinition_LackingCanonicalResourceProperties()
        {
            var definition = new ChargeItemDefinition
            {
                Id = "canonical-properties-test",
                Url = "http://example.org/fhir/ChargeItemDefinition/canonical-test",
                Status = PublicationStatus.Active,
                Title = "Test Definition",
                Version = "1.0.0"
            };

            // Check which canonical resource properties exist
            var hasUrl = definition.GetType().GetProperty("Url") != null;
            var hasVersion = definition.GetType().GetProperty("Version") != null;
            var hasStatus = definition.GetType().GetProperty("Status") != null;
            var hasTitle = definition.GetType().GetProperty("Title") != null;
            var hasDescription = definition.GetType().GetProperty("Description") != null;
            var hasName = definition.GetType().GetProperty("Name",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.IgnoreCase) != null;

            // Verify it has most canonical properties but NOT name
            hasUrl.Should().BeTrue("ChargeItemDefinition should have url");
            hasVersion.Should().BeTrue("ChargeItemDefinition should have version");
            hasStatus.Should().BeTrue("ChargeItemDefinition should have status");
            hasTitle.Should().BeTrue("ChargeItemDefinition should have title");
            hasDescription.Should().BeTrue("ChargeItemDefinition should have description");
            hasName.Should().BeFalse(
                because: "ChargeItemDefinition is missing the 'name' property, which should be a canonical resource property. " +
                "See JHIR-34622 for the resolution that ChargeItemDefinition should be fully canonical-style.");
        }

        /// <summary>
        /// Validates ChargeItemDefinition with all available properties to show
        /// the current state of the resource without the missing "name" field.
        /// </summary>
        [Fact]
        public void ValidateChargeItemDefinitionWithAllAvailableProperties()
        {
            var definition = new ChargeItemDefinition
            {
                Id = "comprehensive-test",
                Url = "http://example.org/fhir/ChargeItemDefinition/comprehensive",
                Version = "2.0.1",
                // NOTE: "Name" property does NOT exist - this is the issue
                Status = PublicationStatus.Active,
                Title = "Comprehensive Charge Item Definition",
                Description = "A comprehensive example with all properties",
                ExperimentalElement = new FhirBoolean(false),
                Date = "2023-06-15",
                PublisherElement = new FhirString("Test Organization"),
            };

            var report = _validator.Validate(definition);

            // Should validate because the resource is valid even without "name"
            report.Success.Should().BeTrue(
                because: "ChargeItemDefinition validates successfully without the 'name' field");
        }
    }
}
