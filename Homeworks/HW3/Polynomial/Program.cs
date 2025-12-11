using System;

class Polynomial
{
    private int degree;
    private double[] coeffs;

    public Polynomial()
    {
        degree = 0;
        coeffs = new double[1] { 0.0 };
    }

    public Polynomial(double[] new_coeffs)
    {
        degree = new_coeffs.Length - 1;
        coeffs = (double[])new_coeffs.Clone();
    }

    public int Degree
    {
        get { return degree; }
    }

    public double[] Coeffs
    {
        get { return (double[])coeffs.Clone(); }
    }

public override string ToString()
    {
        string result = "";
        bool firstTerm = true;

        for (int i = 0; i <= degree; i++)
        {
            double coef = coeffs[i];
            if (coef == 0)
                continue;

            if (firstTerm)
            {
                if (coef < 0)
                    result += "-";
                firstTerm = false;
            }
            else
            {
                result += coef < 0 ? " - " : " + ";
            }

            double absCoef = Math.Abs(coef);

            if (i == 0)
            {
                result += absCoef.ToString();
            }
            else if (i == 1)
            {
                if (absCoef != 1.0)
                    result += absCoef.ToString() + "x";
                else
                    result += "x";
            }
            else
            {
                if (absCoef != 1.0)
                    result += absCoef.ToString() + "x^" + i;
                else
                    result += "x^" + i;
            }
        }

        if (result == "")
            result = "0";

        return result;
    }
}

class Programm
{
    static void Main(string[] args)
    {
        double[] coeffs = { 1.0, 0.0, 2.0 };
        Polynomial p = new Polynomial(coeffs); // 1 + 2x^2

        Console.WriteLine(p);
    }
}