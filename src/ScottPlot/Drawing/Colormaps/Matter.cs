﻿//This is a cmocean colormap
//All credit to Kristen Thyng
//This colormap is under the MIT License
//All cmocean colormaps are available at https://github.com/matplotlib/cmocean/tree/master/cmocean/rgb

namespace ScottPlot.Drawing.Colormaps
{
    public class Matter : IColormap
    {
        public (byte r, byte g, byte b) GetRGB(byte value) =>
            (cmaplocal[value, 0], cmaplocal[value, 1], cmaplocal[value, 2]);

        private static readonly byte[,] cmaplocal = {
            { 254, 237, 176 },
            { 253, 236, 175 },
            { 253, 234, 173 },
            { 253, 233, 172 },
            { 253, 232, 171 },
            { 253, 230, 169 },
            { 253, 229, 168 },
            { 253, 227, 167 },
            { 253, 226, 165 },
            { 252, 225, 164 },
            { 252, 223, 163 },
            { 252, 222, 161 },
            { 252, 220, 160 },
            { 252, 219, 159 },
            { 252, 218, 157 },
            { 252, 216, 156 },
            { 251, 215, 155 },
            { 251, 213, 154 },
            { 251, 212, 152 },
            { 251, 211, 151 },
            { 251, 209, 150 },
            { 251, 208, 148 },
            { 251, 207, 147 },
            { 250, 205, 146 },
            { 250, 204, 145 },
            { 250, 202, 144 },
            { 250, 201, 142 },
            { 250, 200, 141 },
            { 250, 198, 140 },
            { 250, 197, 139 },
            { 249, 195, 138 },
            { 249, 194, 136 },
            { 249, 193, 135 },
            { 249, 191, 134 },
            { 249, 190, 133 },
            { 249, 189, 132 },
            { 248, 187, 131 },
            { 248, 186, 130 },
            { 248, 184, 128 },
            { 248, 183, 127 },
            { 248, 182, 126 },
            { 247, 180, 125 },
            { 247, 179, 124 },
            { 247, 178, 123 },
            { 247, 176, 122 },
            { 247, 175, 121 },
            { 246, 174, 120 },
            { 246, 172, 119 },
            { 246, 171, 118 },
            { 246, 169, 117 },
            { 245, 168, 116 },
            { 245, 167, 115 },
            { 245, 165, 114 },
            { 245, 164, 113 },
            { 245, 163, 112 },
            { 244, 161, 111 },
            { 244, 160, 110 },
            { 244, 159, 109 },
            { 244, 157, 108 },
            { 243, 156, 107 },
            { 243, 154, 106 },
            { 243, 153, 105 },
            { 242, 152, 104 },
            { 242, 150, 104 },
            { 242, 149, 103 },
            { 242, 148, 102 },
            { 241, 146, 101 },
            { 241, 145, 100 },
            { 241, 143, 99 },
            { 240, 142, 99 },
            { 240, 141, 98 },
            { 240, 139, 97 },
            { 239, 138, 96 },
            { 239, 137, 96 },
            { 239, 135, 95 },
            { 238, 134, 94 },
            { 238, 133, 94 },
            { 238, 131, 93 },
            { 237, 130, 92 },
            { 237, 129, 92 },
            { 237, 127, 91 },
            { 236, 126, 90 },
            { 236, 124, 90 },
            { 236, 123, 89 },
            { 235, 122, 89 },
            { 235, 120, 88 },
            { 234, 119, 88 },
            { 234, 118, 87 },
            { 233, 116, 87 },
            { 233, 115, 86 },
            { 233, 114, 86 },
            { 232, 112, 86 },
            { 232, 111, 85 },
            { 231, 110, 85 },
            { 231, 108, 85 },
            { 230, 107, 84 },
            { 230, 106, 84 },
            { 229, 104, 84 },
            { 229, 103, 84 },
            { 228, 102, 83 },
            { 227, 100, 83 },
            { 227, 99, 83 },
            { 226, 98, 83 },
            { 226, 96, 83 },
            { 225, 95, 83 },
            { 224, 94, 83 },
            { 224, 93, 83 },
            { 223, 91, 83 },
            { 223, 90, 83 },
            { 222, 89, 83 },
            { 221, 88, 83 },
            { 220, 86, 83 },
            { 220, 85, 83 },
            { 219, 84, 83 },
            { 218, 83, 83 },
            { 217, 81, 83 },
            { 217, 80, 83 },
            { 216, 79, 84 },
            { 215, 78, 84 },
            { 214, 77, 84 },
            { 213, 76, 84 },
            { 213, 75, 84 },
            { 212, 74, 85 },
            { 211, 72, 85 },
            { 210, 71, 85 },
            { 209, 70, 86 },
            { 208, 69, 86 },
            { 207, 68, 86 },
            { 206, 67, 86 },
            { 205, 66, 87 },
            { 204, 65, 87 },
            { 203, 64, 87 },
            { 202, 63, 88 },
            { 201, 62, 88 },
            { 200, 61, 88 },
            { 199, 61, 89 },
            { 198, 60, 89 },
            { 197, 59, 89 },
            { 196, 58, 90 },
            { 195, 57, 90 },
            { 194, 56, 90 },
            { 193, 55, 91 },
            { 192, 54, 91 },
            { 191, 54, 91 },
            { 190, 53, 92 },
            { 189, 52, 92 },
            { 187, 51, 92 },
            { 186, 50, 93 },
            { 185, 50, 93 },
            { 184, 49, 93 },
            { 183, 48, 94 },
            { 182, 47, 94 },
            { 181, 47, 94 },
            { 179, 46, 95 },
            { 178, 45, 95 },
            { 177, 45, 95 },
            { 176, 44, 95 },
            { 175, 43, 96 },
            { 174, 43, 96 },
            { 172, 42, 96 },
            { 171, 41, 96 },
            { 170, 41, 97 },
            { 169, 40, 97 },
            { 167, 40, 97 },
            { 166, 39, 97 },
            { 165, 38, 98 },
            { 164, 38, 98 },
            { 163, 37, 98 },
            { 161, 37, 98 },
            { 160, 36, 98 },
            { 159, 36, 98 },
            { 158, 35, 99 },
            { 156, 35, 99 },
            { 155, 34, 99 },
            { 154, 34, 99 },
            { 153, 34, 99 },
            { 151, 33, 99 },
            { 150, 33, 99 },
            { 149, 32, 99 },
            { 147, 32, 99 },
            { 146, 31, 99 },
            { 145, 31, 99 },
            { 144, 31, 99 },
            { 142, 30, 99 },
            { 141, 30, 99 },
            { 140, 30, 99 },
            { 138, 29, 99 },
            { 137, 29, 99 },
            { 136, 29, 99 },
            { 134, 29, 99 },
            { 133, 28, 99 },
            { 132, 28, 99 },
            { 130, 28, 99 },
            { 129, 28, 99 },
            { 128, 27, 98 },
            { 126, 27, 98 },
            { 125, 27, 98 },
            { 124, 27, 98 },
            { 122, 27, 98 },
            { 121, 26, 97 },
            { 120, 26, 97 },
            { 118, 26, 97 },
            { 117, 26, 97 },
            { 116, 26, 96 },
            { 114, 26, 96 },
            { 113, 25, 96 },
            { 112, 25, 95 },
            { 110, 25, 95 },
            { 109, 25, 94 },
            { 107, 25, 94 },
            { 106, 25, 94 },
            { 105, 25, 93 },
            { 103, 25, 93 },
            { 102, 24, 92 },
            { 101, 24, 92 },
            { 99, 24, 91 },
            { 98, 24, 91 },
            { 97, 24, 90 },
            { 95, 24, 90 },
            { 94, 24, 89 },
            { 93, 23, 88 },
            { 91, 23, 88 },
            { 90, 23, 87 },
            { 89, 23, 87 },
            { 87, 23, 86 },
            { 86, 23, 85 },
            { 85, 23, 85 },
            { 83, 22, 84 },
            { 82, 22, 83 },
            { 81, 22, 83 },
            { 79, 22, 82 },
            { 78, 22, 81 },
            { 77, 21, 81 },
            { 75, 21, 80 },
            { 74, 21, 79 },
            { 73, 21, 78 },
            { 71, 21, 78 },
            { 70, 20, 77 },
            { 69, 20, 76 },
            { 68, 20, 75 },
            { 66, 20, 75 },
            { 65, 19, 74 },
            { 64, 19, 73 },
            { 62, 19, 72 },
            { 61, 19, 71 },
            { 60, 18, 71 },
            { 59, 18, 70 },
            { 57, 18, 69 },
            { 56, 17, 68 },
            { 55, 17, 67 },
            { 54, 17, 66 },
            { 52, 17, 65 },
            { 51, 16, 65 },
            { 50, 16, 64 },
            { 48, 15, 63 },
            { 47, 15, 62 }

        };
    }
}

