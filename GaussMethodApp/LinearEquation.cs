using System.Collections.Generic;

namespace GaussianElimination
{
    public class LinearEquation
    {
        public LinearEquation(List<double> aMembers, double bMember)
        {
            this.AMembers = aMembers;
            this.BMember = bMember;
        }

        public List<double> AMembers { get; private set; }
        public double BMember { get; set; }
    }
}