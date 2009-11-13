﻿using System.Linq;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Models.Driver;
using Orchard.Roles.Models.NoRecord;
using Orchard.Roles.Services;
using Orchard.Roles.ViewModels;
using Orchard.Security;
using Orchard.UI.Models;
using Orchard.UI.Notify;

namespace Orchard.Roles.Models {
    public class UserRolesModelDriver : ModelDriver {
        private readonly IRepository<UserRolesRecord> _userRolesRepository;
        private readonly IRoleService _roleService;
        private readonly INotifier _notifier;

        public UserRolesModelDriver(IRepository<UserRolesRecord> userRolesRepository, IRoleService roleService, INotifier notifier) {
            _userRolesRepository = userRolesRepository;
            _roleService = roleService;
            _notifier = notifier;
        }

        protected override void New(NewModelContext context) {
            if (context.ModelType == "user") {
                WeldModelPart<UserRolesModel>(context);
            }
        }

        protected override void Create(CreateModelContext context) {
            var userRoles = context.Instance.As<UserRolesModel>();
            if (userRoles != null) {
            }
        }

        protected override void Load(LoadModelContext context) {
            var userRoles = context.Instance.As<UserRolesModel>();
            if (userRoles != null) {
                userRoles.Roles = _userRolesRepository.Fetch(x => x.UserId == userRoles.Id)
                    .Select(x => x.Role.Name).ToList();
            }
        }

        protected override void GetEditors(GetModelEditorsContext context) {
            var userRoles = context.Instance.As<UserRolesModel>();
            if (userRoles != null) {
                var roles =
                    _roleService.GetRoles().Select(
                        x => new UserRoleEntry {
                            RoleId = x.Id,
                            Name = x.Name,
                            Granted = userRoles.Roles.Contains(x.Name)
                        });

                var viewModel = new UserRolesViewModel {
                    User = userRoles.As<IUser>(),
                    UserRoles = userRoles,
                    Roles = roles.ToList(),
                };

                context.Editors.Add(ModelEditor.For("UserRoles", viewModel));
            }
        }

        protected override void UpdateEditors(UpdateModelContext context) {
            var userRoles = context.Instance.As<UserRolesModel>();
            if (userRoles != null) {
                var viewModel = new UserRolesViewModel();
                if (context.Updater.TryUpdateModel(viewModel, null, null, null)) {

                    var currentUserRoleRecords = _userRolesRepository.Fetch(x => x.UserId == userRoles.Id);
                    var currentRoleRecords = currentUserRoleRecords.Select(x => x.Role);
                    var targetRoleRecords = viewModel.Roles.Where(x => x.Granted).Select(x => _roleService.GetRole(x.RoleId));

                    foreach (var addingRole in targetRoleRecords.Where(x => !currentRoleRecords.Contains(x))) {
                        _notifier.Warning(string.Format("Adding role {0} to user {1}", addingRole.Name, userRoles.As<IUser>().UserName));
                        _userRolesRepository.Create(new UserRolesRecord { UserId = userRoles.Id, Role = addingRole });
                    }

                    foreach (var removingRole in currentUserRoleRecords.Where(x => !targetRoleRecords.Contains(x.Role))) {
                        _notifier.Warning(string.Format("Removing role {0} from user {1}", removingRole.Role.Name, userRoles.As<IUser>().UserName));
                        _userRolesRepository.Delete(removingRole);
                    }

                }
                context.Editors.Add(ModelEditor.For("UserRoles", viewModel));
            }
        }
    }
}
