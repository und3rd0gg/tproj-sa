using System;

namespace Obi
{
    // Abstracts rope topolgy as a list of elements.
    [Serializable]
    public class ObiStructuralElement
    {
        public int particle1;
        public int particle2;
        public float restLength;
        public float constraintForce;
        public float tearResistance;
    }
}