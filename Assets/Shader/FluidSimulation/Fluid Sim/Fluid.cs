using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fluid : MonoBehaviour
{
    int size;
    float dt;
    float diff;
    float visc;

    float[] s;
    float[] density;

    float[] Vx;
    float[] Vy;

    float[] Vx0;
    float[] Vy0;
    
    public static int IX(int x, int y, int Size)
    {
        return x + y * Size;
    }
    
    //runtime value
    public Fluid Create(int size, int diffusion, int viscosity, float dt)
    {
        int N = size;
    
        this.size = size;
        this.dt = dt;
        this.diff = diffusion;
        this.visc = viscosity;
    
        this.s = new float[N * N];
        this.density = new float[N * N];
    
        this.Vx = new float[N * N];
        this.Vy = new float[N * N];
    
        this.Vx0 = new float[N * N];
        this.Vy0 = new float[N * N];
    
        return this;
    }
    public void AddDensity(Fluid[] cube, int x, int y, int z, float amount)
    {
        int N = this.size;
        this.density[IX(x, y, size)] += amount;
    }
    public void AddVelocity(Fluid[] cube, int x, int y, int z, float amountX, float amountY, float amountZ)
    {
        int N = this.size;
        int index = IX(x, y, size);
    
        this.Vx[index] += amountX;
        this.Vy[index] += amountY;
    }
    
    //Simulate
    public void FluidCubeStep(Fluid[] cube)
    {
        int N          = this.size;
        float visc     = this.visc;
        float diff     = this.diff;
        float dt       = this.dt;
        float[] Vx      = this.Vx;
        float[] Vy      = this.Vy;
        float[] Vx0     = this.Vx0;
        float[] Vy0     = this.Vy0;
        float[] s       = this.s;
        float[] density = this.density;
    
        diffuse(1, Vx0, Vx, visc, dt, 4, N);
        diffuse(2, Vy0, Vy, visc, dt, 4, N);
    
        project(Vx0, Vy0, Vx, Vy, 4, N);
    
        advect(1, Vx, Vx0, Vx0, Vy0, dt, N);
        advect(2, Vy, Vy0, Vx0, Vy0, dt, N);
    
        project(Vx, Vy, Vx0, Vy0, 4, N);
    
        diffuse(0, s, density, diff, dt, 4, N);
        advect(0, density, s, Vx, Vy, dt, N);
    }
    
    //Set Boundary
    static void set_bnd(int b, float[] x, int N)
    {
        for(int j = 1; j < N - 1; j++) {
            for(int i = 1; i < N - 1; i++) {
                x[IX(i, j, 0  )] = b == 3 ? -x[IX(i, j, 1  )] : x[IX(i, j, 1  )];
                x[IX(i, j, N-1)] = b == 3 ? -x[IX(i, j, N-2)] : x[IX(i, j, N-2)];
            }
        }
        for(int k = 1; k < N - 1; k++) {
            for(int i = 1; i < N - 1; i++) {
                x[IX(i, 0  , k)] = b == 2 ? -x[IX(i, 1  , k)] : x[IX(i, 1  , k)];
                x[IX(i, N-1, k)] = b == 2 ? -x[IX(i, N-2, k)] : x[IX(i, N-2, k)];
            }
        }
        for(int k = 1; k < N - 1; k++) {
            for(int j = 1; j < N - 1; j++) {
                x[IX(0  , j, k)] = b == 1 ? -x[IX(1  , j, k)] : x[IX(1  , j, k)];
                x[IX(N-1, j, k)] = b == 1 ? -x[IX(N-2, j, k)] : x[IX(N-2, j, k)];
            }
        }
        
        x[IX(0, 0, 0)]       = 0.33f * (x[IX(1, 0, 0)]
                                      + x[IX(0, 1, 0)]);
        x[IX(0, N-1, 0)]     = 0.33f * (x[IX(1, N-1, 0)]
                                      + x[IX(0, N-2, 0)]);
        x[IX(0, 0, N-1)]     = 0.33f * (x[IX(1, 0, N-1)]
                                      + x[IX(0, 1, N-1)]);
        x[IX(0, N-1, N-1)]   = 0.33f * (x[IX(1, N-1, N-1)]
                                      + x[IX(0, N-2, N-1)]);
        x[IX(N-1, 0, 0)]     = 0.33f * (x[IX(N-2, 0, 0)]
                                      + x[IX(N-1, 1, 0)]);
        x[IX(N-1, N-1, 0)]   = 0.33f * (x[IX(N-2, N-1, 0)]
                                      + x[IX(N-1, N-2, 0)]);
        x[IX(N-1, 0, N-1)]   = 0.33f * (x[IX(N-2, 0, N-1)]
                                      + x[IX(N-1, 1, N-1)]);
        x[IX(N-1, N-1, N-1)] = 0.33f * (x[IX(N-2, N-1, N-1)]
                                      + x[IX(N-1, N-2, N-1)]);
    }
    
    //Diffuse
    static void diffuse (int b, float[] x, float[] x0, float diff, float dt, int iter, int N)
    {
        float a = dt * diff * (N - 2) * (N - 2);
        lin_solve(b, x, x0, a, 1 + 6 * a, iter, N);
    }
    static void lin_solve(int b, float[] x, float[] x0, float a, float c, int iter, int Size)
    {
        float cRecip = 1 / c;
        for (int k = 0; k < iter; k++) {
            for (int m = 1; m < Size - 1; m++) {
                for (int j = 1; j < Size - 1; j++) {
                    for (int i = 1; i < Size - 1; i++) {
                        x[IX(i, j, m)] =
                            (x0[IX(i, j, m)]
                                + a*(x[IX(i+1, j  , m  )]
                                      +x[IX(i-1, j  , m  )]
                                      +x[IX(i  , j+1, m  )]
                                      +x[IX(i  , j-1, m  )]
                             )) * cRecip;
                    }
                }
            }
            set_bnd(b, x, Size);
        }
    }
    
    //project
    static void project(float[] velocX, float[] velocY, float[] p, float[] div, int iter, int Size)
    {
        for (int k = 1; k < Size - 1; k++) {
            for (int j = 1; j < Size - 1; j++) {
                for (int i = 1; i < Size - 1; i++) {
                    div[IX(i, j, k)] = -0.5f*(
                        velocX[IX(i+1, j  , k  )]
                        -velocX[IX(i-1, j  , k  )]
                        +velocY[IX(i  , j+1, k  )]
                        -velocY[IX(i  , j-1, k  )]
                    )/Size;
                    p[IX(i, j, k)] = 0;
                }
            }
        }
        set_bnd(0, div, Size); 
        set_bnd(0, p, Size);
        lin_solve(0, p, div, 1, 6, iter, Size);
    
        for (int k = 1; k < Size - 1; k++) {
            for (int j = 1; j < Size - 1; j++) {
                for (int i = 1; i < Size - 1; i++) {
                    velocX[IX(i, j, k)] -= 0.5f * (  p[IX(i+1, j, k)]
                                                     -p[IX(i-1, j, k)]) * Size;
                    velocY[IX(i, j, k)] -= 0.5f * (  p[IX(i, j+1, k)]
                                                     -p[IX(i, j-1, k)]) * Size;
                }
            }
        }
        set_bnd(1, velocX, Size);
        set_bnd(2, velocY, Size);
    }
    
    //add Vector
    static void advect(int b, float[] d, float[] d0, float[] velocX, float[] velocY, float dt, int Size)
    {
        float i0, i1, j0, j1;
        float dtx = dt * (Size - 2);
        float dty = dt * (Size - 2);
        float s0, s1, t0, t1;
        float tmp1, tmp2, x, y;
        float Nfloat = Size;
        float ifloat, jfloat, kfloat;
        int i, j, k;

        for (k = 1, kfloat = 1; k < Size - 1; k++, kfloat++)
        {
            for (j = 1, jfloat = 1; j < Size - 1; j++, jfloat++)
            {
                for (i = 1, ifloat = 1; i < Size - 1; i++, ifloat++)
                {
                    tmp1 = dtx * velocX[IX(i, j, k)];
                    tmp2 = dty * velocY[IX(i, j, k)];

                    x = ifloat - tmp1;
                    y = jfloat - tmp2;

                    if (x < 0.5f) x = 0.5f;
                    if (x > Nfloat + 0.5f) x = Nfloat + 0.5f;
                    i0 = (float)Mathf.Floor(x);
                    i1 = i0 + 1.0f;

                    if (y < 0.5f) y = 0.5f;
                    if (y > Nfloat + 0.5f) y = Nfloat + 0.5f;

                    j0 = (float)Mathf.Floor(y);
                    j1 = j0 + 1.0f;
                    s1 = x - i0;
                    s0 = 1.0f - s1;
                    t1 = y - j0;
                    t0 = 1.0f - t1;

                    int i0i = (int)i0;
                    int i1i = (int)i1;
                    int j0i = (int)j0;
                    int j1i = (int)j1;

                    d[IX(i, j, k)] =
                        s0 * (t0 * d0[IX(i0i, j0i, k)] + t1 * d0[IX(i0i, j1i, k)]) +
                        s1 * (t0 * d0[IX(i1i, j0i, k)] + t1 * d0[IX(i1i, j1i, k)]);
                }
            }
        }

        set_bnd(b, d, Size);
    }
}
