namespace Ficto.Models.Helpers;

public abstract record PageBlockViewModel
{
    public abstract string PartialViewName { get; }
}