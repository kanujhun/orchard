﻿using Orchard.Mvc.ViewModels;
using Orchard.Pages.Models;

namespace Orchard.Pages.ViewModels {
    public class PageCreateViewModel : BaseViewModel {
        public ContentItemViewModel<Page> Page { get; set; }
        public bool PromoteToHomePage { get; set; }
    }
}