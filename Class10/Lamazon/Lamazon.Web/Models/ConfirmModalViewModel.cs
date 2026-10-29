namespace Lamazon.Web.Models;

public class ConfirmModalViewModel
{
    public string ModalId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public int EntityId { get; set; }

    public string ConfirmText { get; set; } = "Confirm";

    public string ConfirmCssClass { get; set; } = "btn-danger";

}
