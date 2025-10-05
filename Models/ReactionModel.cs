namespace ChemistryPlus.Models;

public class ReactionModel(Reaction reaction, double[] producedMoles)
{
    public Reaction Reaction { get; private set; } = reaction;
    public double[] ProducedMoles { get; private set; } = producedMoles;
}