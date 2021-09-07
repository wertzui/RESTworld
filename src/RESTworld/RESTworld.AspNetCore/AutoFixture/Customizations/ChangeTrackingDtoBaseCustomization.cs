using AutoFixture;
using RESTworld.Common.Dtos;
using System;

namespace RESTworld.AspNetCore.AutoFixture.Customizations
{
    /// <summary>
    /// Customizes the creation of <see cref="ChangeTrackingDtoBase"/>s properties to contain meaningfull values.
    /// </summary>
    /// <seealso cref="ICustomization" />
    public class ChangeTrackingDtoBaseCustomization : ICustomization
    {
        private static readonly string[] _names = new[]
        {
            "Adam",
            "Bob",
            "Charlie",
            "Diva",
            "Eve"
        };

        private static readonly Random _random = new Random();

        /// <inheritdoc/>
        public void Customize(IFixture fixture)
        {
            fixture
                .CustomizeProperties<ChangeTrackingDtoBase>(composer =>
                    composer
                        .With(d => d.CreatedAt, () => DateTime.Today.AddDays(-1))
                        .With(d => d.LastChangedAt, () => DateTime.Today)
                        .With(d => d.CreatedBy, () => _names[_random.Next(_names.Length)])
                        .With(d => d.LastChangedBy, () => _names[_random.Next(_names.Length)]));
        }
    }
}