﻿using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Logging;

namespace Orchard.ContentManagement.Drivers.Coordinators {
    [UsedImplicitly]
    public class ContentItemDriverCoordinator : ContentHandlerBase {
        private readonly IEnumerable<IContentItemDriver> _drivers;

        public ContentItemDriverCoordinator(IEnumerable<IContentItemDriver> drivers) {
            _drivers = drivers;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override IEnumerable<ContentType> GetContentTypes() {
            var contentTypes = new List<ContentType>();
            _drivers.Invoke(driver=>contentTypes.AddRange(driver.GetContentTypes()), Logger);
            return contentTypes;
        }

        public override void GetContentItemMetadata(GetContentItemMetadataContext context) {        
            _drivers.Invoke(driver => driver.GetContentItemMetadata(context), Logger);
        }

        public override void BuildDisplayModel(BuildDisplayModelContext context) {
            _drivers.Invoke(driver => {
                var result = driver.BuildDisplayModel(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public override void BuildEditorModel(BuildEditorModelContext context) {
            _drivers.Invoke(driver => {
                var result = driver.BuildEditorModel(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public override void UpdateEditorModel(UpdateEditorModelContext context) {
            _drivers.Invoke(driver => {
                var result = driver.UpdateEditorModel(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }
    }
}