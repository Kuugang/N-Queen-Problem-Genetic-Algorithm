using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections;

namespace ACT4
{
    public partial class Form1 : Form
    {
        int side;
        int n = 6;
        SixState startState;
        SixState currentState;
        int moveCounter;
        int populationSize = 50;

        //bool stepMove = true;

        int[,] hTable;
        ArrayList bMoves;
        Object chosenMove;

        List<SixState> population;
        List <SixState> parents;
        List <SixState> children;
        List <SixState> mutated;
        bool valid = false;
        int generation = 0;

        public Form1()
        {
            InitializeComponent();
            
            side = pictureBox1.Width / n;

            startState = randomSixState();
            currentState = startState;
            pictureBox2.Refresh();

            population = initializePopulationFromStartState(startState, populationSize);
            updateUI();
            label2.Text = "Generation: " + generation;
            label3.Text = "";
            output.Text = "";
            label1.Text = "Start State";
        }
        
        private SixState getMaxFitnessState(List<SixState> states)
        {
            int maxFitness = 0;

            SixState state = null;

            for (int i = 0; i < states.Count; i++)
            {
                int fitness = calculateFitness(states[i]);
                if (fitness > maxFitness)
                {
                    maxFitness = fitness;
                    state = states[i];
                }
            }
            return state;
        }

        private bool checkValid(List<SixState> states)
        {
            for (int i = 0; i < states.Count; i++)
            {
                if (getAttackingPairs(states[i]) == 0)
                    return true;
            }
            return false;
        }

        private void updateUI() { 
            label2.Text = "Generation: " + generation;

            String s = "State with max fitness: ";
            for(int i = 0; i < n; i++)
            {
                s += currentState.Y[i] + " ";
            }
            label3.Text = s;

            output.AppendText("Generation: " + generation + "  |  Max Fitness: " + calculateFitness(currentState));
            output.AppendText(Environment.NewLine);

            pictureBox2.Refresh();
            
            //foreach (SixState p in population)
            //{
                //String s = "";
                //for (int i = 0; i < n; i++)
                //{
                //    s += p.Y[i] + " ";
                //}
                //s += " Fitness: " + calculateFitness(p);
                //output.AppendText(s);
                //output.AppendText(Environment.NewLine);
            //}
        }
        private void nextGeneration()
        {
            parents = selectParents(population);
            children = crossover(parents);
            valid = checkValid(children);

            if (valid)
            {
                population = children;
                currentState = getMaxFitnessState(population);
                updateUI();
                return;
            }

            mutated = mutate(children);
            valid = checkValid(mutated);

            population = mutated;
            if (valid)
            {
                currentState = getMaxFitnessState(population);
                updateUI();
                return;
            }

            currentState = getMaxFitnessState(population);

            generation += 1;
            updateUI();
        }

        private List<SixState> initializePopulationFromStartState(SixState startState, int populationSize)
        {
            List<SixState> population = new List<SixState>();

            Random r = new Random();
            population.Add(new SixState(startState));

            for (int i = 1; i < populationSize; i++)
            {
                SixState newMember = new SixState(startState);
                newMember.Y[r.Next(n)] = r.Next(n);
                population.Add(newMember);
            }

            return population;
        }

        private int calculateFitness(SixState state)
        {
            return (n*(n-1) / 2) - getAttackingPairs(state);
        }

        private List<SixState> selectParents(List<SixState> population)
        {
            Random r = new Random();
            int totalFitness = 0;
            foreach (SixState member in population){
                totalFitness += calculateFitness(member);
            }

            List<float> probabilities = new List<float>();

            foreach (SixState member in population)
            {
                probabilities.Add((float)calculateFitness(member) / totalFitness);
            }

            double randomValue = r.NextDouble();
            double cumulative = 0;
            List <SixState> parents = new List<SixState>();
            
            while(parents.Count < populationSize)
            {

                for(int i = 0; i < probabilities.Count; i++)
                {
                    if(parents.Count >= populationSize)
                        break;
                    cumulative += probabilities[i];
                    if (randomValue < cumulative)
                    {
                        parents.Add(population[i]);
                    }
                }
            }

            return parents;
        }

        private List<SixState> crossover(List<SixState> parents)
        {
            List<SixState> children = new List<SixState>();
            Random r = new Random();
            for (int i = 0; i < parents.Count; i++)
            {
                SixState parent1 = parents[i];
                SixState parent2 = parents[(i + 1) % parents.Count];
                SixState child = new SixState();

                for (int j = 0; j < n; j++)
                {
                    if (r.NextDouble() < 0.5)
                        child.Y[j] = parent1.Y[j];
                    else
                        child.Y[j] = parent2.Y[j];
                }

                children.Add(child);
            }
            return children;
        }

        private List<SixState> mutate(List<SixState> children)
        {
            Random r = new Random();
            for (int i = 0; i < children.Count; i++)
            {
                SixState child = children[i];
                if (r.NextDouble() < 0.1)
                {
                    child.Y[r.Next(n)] = r.Next(n);
                }
            }
            return children;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(valid == false)
            {
                nextGeneration();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            startState = randomSixState();
            currentState =startState;

            moveCounter = 0;

            updateUI();
            pictureBox1.Refresh();
            output.Text = "";
            label2.Text = "Generation: 0";
            label3.Text = "";
            generation = 0;
            population = initializePopulationFromStartState(startState, populationSize);
            valid = false;
            pictureBox2.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (valid == false)
            {
                nextGeneration();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }      
        
        private int getAttackingPairs(SixState f)
        {
            int attackers = 0;
            
            for (int rf = 0; rf < n; rf++)
            {
                for (int tar = rf+1; tar < n; tar++)
                {
                    // get horizontal attackers
                    if (f.Y[rf] == f.Y[tar])
                        attackers++;
                }
                for (int tar = rf+1; tar < n; tar++)
                {
                    // get diagonal down attackers
                    if (f.Y[tar] == f.Y[rf] + tar - rf)
                        attackers++;
                }
                for (int tar = rf+1; tar < n; tar++)
                {
                    // get diagonal up attackers
                    if (f.Y[rf] == f.Y[tar] + tar - rf)
                        attackers++;
                }
            }
            
            return attackers;
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // draw squares
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == startState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }
        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            // draw squares
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Black, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == currentState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private SixState randomSixState()
        {
            Random r = new Random();
            SixState random = new SixState(r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n));

            return random;
        }

        private void validatePopulationSizeInput(object sender, EventArgs e)
        {
            if (startState != currentState && valid == false)
            {
                textBox1.Text = populationSize.ToString();
                return;
            }
            else
            {
                try
                {
                    populationSize = Convert.ToInt32(textBox1.Text);
                    population = initializePopulationFromStartState(startState, populationSize);
                }catch (Exception ex)
                {
                    textBox1.Text = populationSize.ToString();
                }
            }
        }
    }
}
