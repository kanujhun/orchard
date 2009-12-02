using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Models;
using Orchard.Settings;
using Orchard.Tags.Models;
using Orchard.Tags.Services;
using Orchard.Tags.ViewModels;
using Orchard.UI.Notify;
using Orchard.Security;

namespace Orchard.Tags.Controllers {
    [ValidateInput(false)]
    public class HomeController : Controller {
        private readonly ITagService _tagService;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;

        public HomeController(ITagService tagService, INotifier notifier, IAuthorizer authorizer) {
            _tagService = tagService;
            _authorizer = authorizer;
            _notifier = notifier;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IUser CurrentUser { get; set; }
        public ISite CurrentSite { get; set; }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            try {
                var tags = _tagService.GetTags();
                var model = new TagsIndexViewModel { Tags = tags.ToList() };
                return View(model);
            }
            catch (Exception exception) {
                _notifier.Error(T("Listing tags failed: " + exception.Message));
                return Index();
            }
        }

        [HttpPost]
        public ActionResult Edit(FormCollection input, int taggedContentId, string returnUrl, string newTagName) {
            try {
                if (!_authorizer.Authorize(Permissions.CreateTag, T("Couldn't create tag")))
                    return new HttpUnauthorizedResult();
                if (!String.IsNullOrEmpty(newTagName)) {
                    foreach (var tagName in ParseCommaSeparatedTagNames(newTagName)) {
                        if (_tagService.GetTagByName(tagName) == null) {
                            _tagService.CreateTag(tagName);
                        }
                        _tagService.TagContentItem(taggedContentId, tagName);
                    }
                }
                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error(T("Editing tags failed: " + exception.Message));
                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Update(string tags, int taggedContentId, string returnUrl) {
            try {
                if (!_authorizer.Authorize(Permissions.CreateTag, T("Couldn't create tag")))
                    return new HttpUnauthorizedResult();
                List<string> tagNames = ParseCommaSeparatedTagNames(tags);
                _tagService.UpdateTagsForContentItem(taggedContentId, tagNames);
                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error(T("Updating tags failed: " + exception.Message));
                if (!String.IsNullOrEmpty(returnUrl)) {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
        }

        private static List<string> ParseCommaSeparatedTagNames(string tags) {
            IEnumerable<string> tagNames = tags.Split(',');
            List<string> sanitizedTagNames = new List<string>();
            foreach (var tagName in tagNames) {
                if (!String.IsNullOrEmpty(tagName)) {
                    sanitizedTagNames.Add(tagName);
                }
            }
            return sanitizedTagNames;
        }

        public ActionResult Search(string tagName) {
            try {
                Tag tag = _tagService.GetTagByName(tagName);
                IEnumerable<IContent> contents = _tagService.GetTaggedContentItems(tag.Id).ToList();
                var viewModel = new TagsSearchViewModel {
                    TagName = tag.TagName,
                    Contents = contents,
                };
                return View(viewModel);

            }
            catch (Exception exception) {
                _notifier.Error(T("Retrieving tagged items failed: " + exception.Message));
                return RedirectToAction("Index");
            }
        }
    }
}