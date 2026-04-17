namespace Ficto.Models;

public abstract record PageBlockViewModel
{
    public abstract string PartialViewName { get; }
}