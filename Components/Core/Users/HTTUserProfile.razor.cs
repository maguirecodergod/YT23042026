namespace HTT.BlazorWasm.App.Components
{
    public partial class HTTUserProfile : HTTComponentBase
    {
        [Parameter] public UserBaseModel? Model { get; set; }
        [Parameter] public CUserProfileDisplayMode DisplayMode { get; set; } = CUserProfileDisplayMode.AvatarOnly;
        [Parameter] public CUserProfileInteractionMode InteractionMode { get; set; } = CUserProfileInteractionMode.ReadOnly;
        [Parameter] public string Class { get; set; } = string.Empty;
        [Parameter] public string Style { get; set; } = string.Empty;
        [Parameter] public CPositionType TooltipPosition { get; set; } = CPositionType.Bottom;
        [Parameter] public bool EnableToolTip { get; set; } = false;
        [Parameter] public EventCallback<UserBaseModel> OnClick { get; set; }

        private bool IsClickable => InteractionMode == CUserProfileInteractionMode.Clickable;
        private bool ShowFullName => DisplayMode == CUserProfileDisplayMode.AvatarAndFullName;


        private HTTToolTip? _tooltip;
        
        private async Task HandleClick()
        {
            _tooltip?.HideImmediate();
            
            if (IsClickable && OnClick.HasDelegate)
            {
                await OnClick.InvokeAsync(Model);
            }
        }

        private string GetAvatarText()
        {
            if (Model == null) return string.Empty;

            if (!string.IsNullOrEmpty(Model.LastName))
            {
                return Model.LastName.Substring(0, 1).ToUpper();
            }
            if (!string.IsNullOrEmpty(Model.FullName))
            {
                var parts = Model.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    return parts.Last().Substring(0, 1).ToUpper();
                }
            }
            if (!string.IsNullOrEmpty(Model.FirstName))
            {
                return Model.FirstName.Substring(0, 1).ToUpper();
            }
            if (!string.IsNullOrEmpty(Model.UserName))
            {
                return Model.UserName.Substring(0, 1).ToUpper();
            }

            return "U";
        }
    }
}
