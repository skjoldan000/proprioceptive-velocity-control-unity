Shader "Custom/AlwaysOnTop"
{
    SubShader
    {
        Tags { "Queue" = "Overlay" } // Render after everything
        Pass
        {
            ZTest Always   // Always pass depth test
            ZWrite Off      // Don't write to depth buffer

            // Use a basic color here for simplicity
            Color (1, 1, 1, 1)
        }
    }
}
