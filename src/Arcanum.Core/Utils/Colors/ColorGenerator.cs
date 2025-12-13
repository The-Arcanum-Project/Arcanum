using static System.Windows.Media.Colors;
using Color = System.Windows.Media.Color;

namespace Arcanum.Core.Utils.Colors;

public static class ColorGenerator
{
   private static readonly int[] LandHueValues =
   [
      70, 231, 127, 219, 121, 7, 287, 289, 120, 268, 192, 271, 40, 224, 111, 130, 284, 328, 101, 100, 23, 45, 108, 189, 49, 20, 299, 41, 229, 206, 218, 132,
      5, 258, 89, 110, 2, 235, 219, 96, 297, 206, 339, 71, 280, 340, 273, 9, 331, 189, 84, 58, 104, 244, 111, 108, 225, 74, 271, 117, 327, 122, 251, 70, 122,
      201, 47, 260, 199, 228, 245, 255, 268, 254, 123, 287, 54, 185, 245, 91, 281, 182, 336, 217, 251, 194, 312, 224, 208, 339, 280, 305, 65, 268, 68, 230,
      218, 242, 77, 85, 333, 203, 203, 257, 286, 340, 74, 220, 81, 62, 29, 328, 80, 304, 119, 203, 297, 337, 132, 105, 56, 25, 323, 1, 91, 217, 36, 4, 12,
      105, 232, 260, 195, 310, 224, 334, 84, 20, 268, 88, 297, 73, 291, 32, 314, 96, 267, 110, 225, 38, 256, 126, 202, 324, 236, 125, 253, 115, 322, 317,
      208, 243, 41, 91, 305, 23, 211, 90, 39, 20, 135, 63, 195, 329, 94, 42, 127, 299, 43, 266, 309, 83, 257, 74, 44, 139, 24, 215, 263, 232, 85, 40, 125,
      255, 267, 57, 299, 186, 318, 190, 205, 287, 16, 338, 29, 24, 69, 338, 1, 35, 337, 255, 203, 34, 34, 189, 277, 97, 302, 296, 227, 43, 292, 326, 202,
      231, 132, 312, 251, 85, 71, 235, 23, 325, 182, 337, 112, 238, 292, 226, 59, 293, 101, 203, 43, 67, 193, 78, 210, 11, 101, 28, 63, 340, 66, 327, 182,
      15, 335, 280, 61, 197, 107, 300, 19, 313, 298, 27, 292, 280, 240, 253, 295, 254, 129, 100, 202, 245, 209, 58, 290, 254, 214, 252, 192, 196, 40, 243,
      218, 195, 211, 99, 2, 289, 16, 183, 85, 302, 278, 20, 80, 7, 182, 281, 210, 250, 119, 239, 102, 51, 205, 231, 264, 12, 258, 119, 248, 244, 139, 219,
      239, 248, 217, 285, 8, 206, 266, 324, 211, 221, 322, 235, 114, 44, 304, 121, 216, 251, 115, 248, 14, 334, 260, 2, 191, 136, 261, 18, 231, 75, 190, 49,
      35, 16, 277, 313, 9, 234, 49, 184, 340, 225, 101, 11, 19, 88, 267, 78, 295, 45, 59, 292, 275, 95, 43, 36, 327, 12, 47, 288, 266, 138, 271, 71, 206,
      215, 80, 210, 225, 224, 312, 100, 209, 283, 21, 200, 307, 294, 102, 339, 336, 41, 58, 228, 16, 74, 301, 247, 37, 111, 118, 66, 266, 56, 62, 37, 112,
      128, 135, 75, 247, 189, 235, 129, 109, 276, 44, 9, 129, 272, 92, 1, 94, 301, 129, 278, 44, 98, 249, 202, 268, 58, 24, 85, 291, 102, 293, 55, 334, 299,
      230, 312, 216, 340, 49, 53, 29, 182, 217, 79, 51, 101, 311, 98, 219, 278, 263, 8, 308, 27, 226, 133, 50, 41, 228, 33, 219, 128, 233, 78, 258, 26, 185,
      245, 263, 39, 261, 58, 337, 186, 245, 299, 287, 284, 329, 217, 117, 289, 260, 110, 38, 322, 308, 57, 111, 291, 45, 284, 14, 31, 338, 72, 90, 324, 53,
      115, 62, 125, 46, 114, 68, 64, 251, 106, 66, 73, 53, 330, 32, 185, 187, 105, 252, 230, 202, 181, 327, 91, 198, 196, 265, 253, 112, 29, 334, 65, 230,
      76, 252, 322, 14, 324, 339, 197, 23, 249, 340, 197, 214, 329, 337, 69, 123, 92, 257, 109, 117, 224, 15, 284, 263, 181, 208, 330, 298, 261, 224, 129,
      190, 47, 184, 102, 107, 120, 195, 185, 295, 58, 207, 302, 18, 318, 289, 38, 183, 34, 298, 252, 37, 297, 31, 6, 77, 17, 9, 320, 300, 206, 98, 292, 276,
      330, 77, 262, 109, 82, 95, 60, 298, 219, 119, 8, 307, 277, 335, 337, 27, 251, 239, 56, 282, 18, 40, 196, 187, 301, 310, 216, 224, 337, 284, 311, 108,
      61, 324, 35, 18, 50, 289, 45, 281, 306, 310, 60, 183, 260, 206, 123, 296, 216, 182, 307, 128, 328, 265, 302, 18, 39, 312, 191, 64, 0, 86, 85, 99, 11,
      94, 229, 212, 273, 53, 321, 86, 3, 71, 35, 297, 245, 261, 339, 187, 266, 269, 233, 19, 84, 53, 40, 95, 274, 129, 93, 65, 106, 234, 186, 325, 37, 270,
      90, 209, 338, 35, 72, 96, 97, 184, 290, 75, 328,
   ];

   private static readonly int[] WaterHueValues =
   [
      492, 407, 414, 490, 491, 344, 354, 491, 457, 476, 512, 364, 370, 378, 412, 394, 368, 439, 438, 496, 381, 426, 486, 369, 367, 459, 436, 442, 507, 464,
      439, 354, 458, 369, 410, 369, 490, 478, 460, 360, 410, 353, 495, 445, 422, 451, 405, 365, 342, 484, 414, 455, 437, 492, 413, 346, 492, 434, 353, 472,
      366, 372, 507, 389, 399, 351, 382, 493, 382, 403, 366, 415, 346, 367, 372, 366, 520, 492, 375, 363, 455, 478, 426, 475, 366, 388, 489, 356, 435, 456,
      503, 363, 407, 514, 515, 426, 440, 519, 437, 344, 481, 485, 508, 397, 464, 369, 484, 461, 491, 351, 380, 400, 380, 387, 468, 485, 494, 432, 379, 421,
      362, 395, 400, 498, 367, 429, 410, 429, 352, 360, 511, 424, 458, 414, 362, 516, 405, 503, 424, 446, 511, 478, 496, 388, 373, 403, 438, 350, 437, 348,
      347, 415, 419, 474, 460, 357, 413, 430, 464, 467, 373, 433, 515, 503, 387, 367, 477, 434, 346, 474, 507, 347, 520, 401, 442, 511, 400, 382, 358, 479,
      500, 423, 378, 482, 457, 415, 490, 472, 417, 463, 457, 468, 380, 352, 407, 388, 346, 478, 507, 479, 371, 365, 343, 358, 392, 439, 483, 484, 504, 486,
      378, 396, 345, 490, 464, 423, 513, 467, 383, 368, 365, 467, 369, 469, 490, 376, 503, 440, 392, 467, 410, 381, 406, 421, 461, 514, 503, 348, 423, 370,
      435, 502, 431, 381, 441, 486, 430, 440, 463, 408, 394, 384, 482, 368, 467, 402, 518, 410, 427, 493, 422, 387, 501, 399, 345, 465, 378, 377, 400, 380,
      434, 445, 349, 438, 386, 362, 517, 453, 365, 365, 391, 343, 410, 415, 441, 368, 360, 476, 480, 344, 490, 371, 418, 453, 447, 399, 501, 349, 451, 471,
      344, 470, 459, 456, 467, 465, 417, 381, 355, 436, 461, 487, 454, 448, 415, 346, 402, 382, 415, 388, 489, 369, 410, 422, 377, 452, 372, 472, 490, 377,
      473, 488, 512, 478, 373, 485, 454, 476, 413, 377, 460, 441, 438, 359, 346, 381, 368, 373, 432, 389, 420, 520, 436, 520, 422, 465, 400, 484, 495, 342,
      400, 341, 406, 359, 348, 350, 491, 421, 358, 512, 370, 474, 411, 397, 380, 519, 517, 430, 355, 465, 488, 506, 421, 444, 348, 466, 453, 480, 416, 453,
      363, 418, 468, 393, 512, 416, 453, 500, 403, 346, 495, 400, 484, 396, 455, 448, 417, 504, 381, 392, 389, 343, 462, 473, 411, 513, 458, 410, 408, 482,
      500, 383, 496, 476, 517, 445, 420, 457, 462, 352, 346, 490, 419, 430, 514, 471, 480, 507, 477, 509, 359, 370, 493, 425, 382, 461, 443, 356, 389, 373,
      389, 468, 470, 444, 432, 495, 355, 347, 360, 361, 416, 405, 361, 419, 446, 388, 443, 379, 485, 404, 446, 340, 347, 440, 443, 371, 398, 477, 516, 399,
      362, 342, 348, 349, 495, 367, 353, 489, 352, 413, 350, 344, 509, 471, 490, 398, 393, 505, 517, 469, 348, 354, 358, 402, 360, 404, 472, 460, 517, 423,
      505, 342, 365, 515, 384, 443, 360, 486, 489, 345, 469, 430, 491, 517, 394, 491, 433, 412, 473, 509, 472, 390, 394, 516, 385, 468, 394, 500, 409, 470,
      508, 453, 361, 357, 437, 431, 516, 519, 419, 349, 472, 519, 438, 390, 504, 387, 511, 421, 415, 467, 496, 356, 412, 492, 350, 506, 396, 428, 431, 489,
      383, 472, 457, 502, 384, 441, 493, 507, 439, 351, 493, 486, 418, 434, 378, 500, 463, 393, 380, 511, 468, 377, 514, 378, 511, 483, 418, 345, 354, 423,
      376, 388, 511, 428, 433, 346, 450, 489, 381, 462, 394, 506, 466, 354, 400, 386, 469, 433, 414, 506, 448, 442, 360, 351, 359, 351, 468, 512, 464, 432,
      475, 472, 475, 377, 483, 401, 432, 358, 470, 389, 480, 464, 434, 494, 427, 361, 409, 406, 394, 491, 360, 503, 500, 445, 443, 366, 399, 450, 364, 374,
      511, 376, 361, 415, 465, 405, 467, 477, 502, 485, 370, 361, 443, 484, 391, 376, 342, 474, 393, 364, 376, 356, 438, 433, 510, 510, 485, 444, 342, 450,
      464, 407, 415, 397, 425, 341, 420, 493, 398, 360, 366, 461, 364, 381, 488, 416, 413, 409, 441, 518, 507, 514, 483, 423, 510, 358, 426, 494, 431, 446,
   ];

   private static readonly int[] SaturationValues =
   [
      148, 628, 596, 661, 554, 553, 982, 894, 684, 608, 936, 741, 732, 590, 280, 563, 505, 119, 974, 412, 570, 525, 793, 572, 323, 874, 946, 365, 687, 93,
      494, 277, 192, 965, 27, 241, 836, 75, 230, 987, 455, 264, 824, 259, 9, 268, 55, 233, 635, 602, 909, 776, 767, 852, 63, 163, 67, 96, 672, 238, 236, 104,
      483, 423, 462, 916, 137, 840, 959, 287, 199, 216, 681, 155, 235, 737, 522, 575, 788, 641, 74, 579, 247, 159, 17, 102, 446, 80, 22, 383, 421, 520, 606,
      849, 474, 419, 480, 134, 895, 674, 441, 272, 257, 555, 745, 643, 556, 116, 328, 627, 657, 881, 12, 953, 735, 111, 825, 445, 698, 508, 337, 222, 364,
      10, 71, 875, 535, 76, 226, 360, 8, 961, 529, 417, 785, 209, 438, 799, 865, 120, 567, 677, 430, 344, 704, 253, 313, 890, 777, 988, 343, 782, 142, 157,
      869, 984, 402, 249, 112, 23, 843, 809, 496, 215, 583, 292, 742, 510, 539, 648, 24, 652, 917, 733, 84, 47, 877, 729, 227, 19, 443, 207, 449, 384, 255,
      345, 649, 738, 265, 891, 389, 274, 498, 72, 460, 620, 399, 991, 152, 225, 178, 428, 604, 283, 591, 523, 410, 198, 68, 324, 463, 88, 489, 750, 448, 25,
      986, 711, 139, 827, 396, 359, 515, 70, 279, 759, 331, 507, 669, 174, 552, 844, 468, 36, 14, 762, 447, 509, 900, 401, 478, 234, 432, 911, 932, 931, 855,
      771, 696, 94, 564, 391, 970, 115, 101, 798, 388, 595, 79, 282, 587, 671, 133, 349, 605, 57, 886, 146, 774, 964, 482, 457, 245, 966, 229, 783, 254, 915,
      327, 382, 317, 654, 963, 795, 237, 803, 839, 228, 223, 65, 176, 706, 173, 768, 819, 857, 346, 938, 252, 298, 304, 334, 167, 306, 607, 910, 458, 586,
      28, 715, 354, 846, 5, 271, 831, 82, 297, 46, 848, 456, 1000, 850, 655, 475, 954, 195, 773, 656, 194, 13, 464, 66, 348, 54, 794, 210, 957, 925, 246, 83,
      377, 975, 511, 618, 796, 752, 985, 883, 854, 481, 310, 666, 191, 251, 920, 312, 266, 295, 823, 617, 232, 540, 316, 971, 801, 49, 610, 376, 374, 994,
      392, 357, 151, 188, 779, 296, 366, 122, 342, 584, 653, 98, 20, 947, 551, 240, 519, 992, 400, 558, 513, 450, 15, 526, 999, 810, 816, 725, 219, 679, 205,
      29, 945, 775, 486, 860, 647, 968, 131, 760, 58, 303, 203, 244, 256, 185, 721, 623, 33, 639, 664, 113, 373, 91, 888, 100, 16, 181, 790, 398, 680, 730,
      950, 659, 822, 700, 211, 126, 52, 248, 404, 301, 405, 871, 325, 756, 637, 309, 350, 952, 394, 60, 335, 713, 630, 939, 370, 527, 537, 845, 541, 69, 640,
      560, 941, 231, 577, 663, 889, 48, 565, 436, 806, 338, 690, 175, 512, 834, 127, 286, 97, 699, 744, 182, 644, 311, 171, 719, 981, 278, 977, 425, 239,
      996, 380, 21, 818, 284, 697, 683, 873, 484, 73, 250, 429, 262, 772, 808, 692, 130, 599, 140,
   ];

   private static readonly int[] ValueValues =
   [
      576, 691, 748, 488, 970, 194, 869, 164, 949, 348, 575, 230, 276, 373, 847, 706, 113, 571, 617, 770, 390, 632, 69, 24, 607, 660, 636, 121, 863, 344,
      328, 253, 957, 31, 178, 496, 257, 204, 619, 221, 381, 144, 686, 750, 82, 985, 905, 417, 47, 54, 285, 86, 690, 540, 593, 610, 489, 41, 108, 217, 784,
      532, 745, 87, 846, 939, 766, 366, 156, 364, 487, 910, 908, 235, 796, 293, 597, 738, 401, 236, 317, 577, 161, 974, 101, 123, 80, 943, 112, 923, 854,
      792, 802, 124, 824, 359, 6, 991, 774, 60, 661, 864, 693, 193, 922, 294, 12, 369, 941, 98, 215, 465, 319, 546, 78, 442, 947, 680, 143, 133, 963, 665,
      944, 600, 2, 705, 840, 242, 109, 238, 379, 965, 229, 606, 900, 321, 626, 484, 898, 801, 842, 42, 136, 89, 498, 520, 72, 505, 246, 818, 450, 865, 349,
      132, 152, 404, 501, 117, 189, 210, 516, 176, 431, 48, 966, 1, 139, 45, 179, 621, 197, 451, 566, 751, 416, 804, 425, 326, 467, 64, 798, 299, 300, 780,
      527, 131, 410, 460, 157, 8, 223, 81, 353, 443, 382, 603, 935, 83, 816, 713, 628, 100, 529, 695, 166, 22, 482, 356, 1000, 572, 776, 716, 476, 67, 440,
      218, 984, 545, 702, 904, 539, 653, 858, 906, 787, 327, 405, 354, 746, 11, 239, 697, 559, 88, 135, 681, 659, 613, 839, 10, 419, 872, 380, 836, 833, 849,
      720, 394, 374, 768, 5, 778, 393, 743, 512, 821, 159, 820, 497, 231, 26, 456, 684, 977, 295, 554, 567, 791, 664, 190, 843, 324, 926, 521, 759, 233, 788,
      721, 757, 192, 742, 481, 790, 595, 120, 866, 203, 303, 588, 525, 625, 671, 871, 803, 341, 424, 860, 96, 782, 585, 662, 764, 362, 388, 616, 275, 578,
      103, 754, 376, 185, 658, 855, 383, 942, 187, 728, 767, 829, 62, 53, 639, 90, 177, 715, 755, 39, 562, 799, 528, 43, 657, 279, 77, 40, 674, 611, 310,
      265, 901, 912, 987, 398, 981, 972, 171, 651, 307, 990, 641, 624, 826, 312, 232, 549, 975, 508, 608, 793, 711, 486, 885, 495, 240, 973, 760, 142, 769,
      222, 857, 34, 461, 959, 817, 806, 94, 777, 582, 447, 464, 357, 994, 758, 32, 262, 948, 873, 122, 490, 84, 812, 332, 688, 772, 531, 962, 564, 271, 642,
      209, 951, 692, 435, 263, 727, 510, 455, 844, 358, 744, 25, 830, 311, 367, 313, 952, 579, 418, 586, 828, 894, 568, 368, 762, 141, 541, 914, 345, 287,
      921, 604, 73, 227, 266, 785, 813, 874, 615, 878, 422, 825, 314, 673, 186, 331, 306, 70, 385, 269, 499, 903, 315, 827, 570, 448, 473, 978, 212, 148,
      583, 371, 244, 988, 517, 9, 630, 986, 270, 403, 763, 771, 779, 811, 683, 712, 52, 730, 775, 173, 254, 195, 250, 449, 278, 558, 325, 924, 678, 710, 794,
      115, 936, 475, 296, 174, 282, 116, 884, 565, 598, 352, 822, 339, 832, 709, 30, 50, 444, 277, 207, 492, 428,
   ];

   private const int PRIME = 397;

   private static readonly Color[] RedGreenGradient100 = GenerateGradient(LawnGreen, DarkRed, 100);

   #region ValueGetterHelpers

   public static int GetHue(bool isLand, int index)
   {
      return isLand
                ? LandHueValues[index * PRIME % LandHueValues.Length]
                // We need to subtract 360 to keep water hues within the 0-360 range. Why? I have no idea.
                : WaterHueValues[index * PRIME % WaterHueValues.Length] - 360;
   }

   public static int GetSaturation(int index)
   {
      // _saturationValues range from 1 to 1000, so we need to scale it down to 0-100
      return SaturationValues[index * PRIME % SaturationValues.Length] / 10;
   }

   public static int GetValue(int index)
   {
      // _valueValues range from 1 to 1000, so we need to scale it down to 0-100
      return ValueValues[index * PRIME % ValueValues.Length] / 10;
   }

   // ReSharper disable once UnusedMember.Local
   private static Color Generate(bool isLand, int index)
   {
      var h = GetHue(isLand, index);
      var s = GetSaturation(index) / 100.0;
      var v = GetValue(index) / 100.0;

      var c = v * s;
      var x = c * (1 - Math.Abs(h / 60 % 2 - 1));
      var m = v - c;

      double r = 0,
             g = 0,
             b = 0;
      (r, g, b) = (h / 60) switch
      {
         0 => (c, x, 0),
         1 => (x, c, 0),
         2 => (0, c, x),
         3 => (0, x, c),
         4 => (x, 0, c),
         5 => (c, 0, x),
         _ => (r, g, b),
      };

      return Color.FromRgb((byte)Math.Round((r + m) * 255),
                           (byte)Math.Round((g + m) * 255),
                           (byte)Math.Round((b + m) * 255));
   }

   #endregion

   public static Color GenerateColor(int index)
   {
      return Generate(false, index);
   }

   public static Color GetRedGreenGradient(float value)
   {
      value = Math.Clamp(value, 0, 1);
      var index = (int)(value * (RedGreenGradient100.Length - 1));
      return RedGreenGradient100[index];
   }

   public static Color GetRedGreenGradientInverse(float value) => GetRedGreenGradient(1 - value);

   /// <summary>
   /// Converts a Color object to its 32-bit ARGB (Alpha, Red, Green, Blue) integer representation.
   /// This is standard for WPF and GDI.
   /// </summary>
   public static int AsArgbInt(this Color color)
   {
      return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
   }

   /// <summary>
   /// Converts a Color object to its 32-bit ABGR (Alpha, Blue, Green, Red) integer representation.
   /// This is common in graphics APIs like DirectX and OpenGL.
   /// </summary>
   public static int AsAbgrInt(this Color color)
   {
      // Notice the R and B channels are swapped in their bitwise positions.
      return (color.A << 24) | (color.B << 16) | (color.G << 8) | color.R;
   }

   /// <summary>
   /// A struct to represent a color in the HSL (Hue, Saturation, Lightness) color space.
   /// </summary>
   private struct HslColor
   {
      public double H; // Hue, from 0 to 360
      public double S; // Saturation, from 0 to 1
      public double L; // Lightness, from 0 to 1
   }

   /// <summary>
   /// Generates a list of colors that are perceptually close to a given base color.
   /// </summary>
   /// <param name="baseColor">The starting color.</param>
   /// <param name="count">The number of color variations to generate.</param>
   /// <param name="saturationVariation">The maximum amount Saturation can change (e.g., 0.1 for +/- 10%).</param>
   /// <param name="lightnessVariation">The maximum amount Lightness can change (e.g., 0.1 for +/- 10%).</param>
   /// <returns>A list of generated Color objects.</returns>
   public static Color[] GenerateVariations(
      Color baseColor,
      int count,
      double saturationVariation = 0.1,
      double lightnessVariation = 0.1)
   {
      if (count <= 0)
         return [];

      var variations = new Color[count];
      var baseHsl = RgbToHsl(baseColor);
      var random = new Random();

      for (var i = 0; i < count; i++)
      {
         // Create a new HSL color based on the original
         var newHsl = baseHsl;

         // Jiggle the Saturation and Lightness within the specified range
         // The formula (random.NextDouble() * 2 - 1) generates a random number between -1 and 1
         var satJiggle = (random.NextDouble() * 2 - 1) * saturationVariation;
         var lightJiggle = (random.NextDouble() * 2 - 1) * lightnessVariation;

         newHsl.S += satJiggle;
         newHsl.L += lightJiggle;

         // Clamp the values to ensure they remain in the valid [0, 1] range
         newHsl.S = Math.Max(0, Math.Min(1, newHsl.S));
         newHsl.L = Math.Max(0, Math.Min(1, newHsl.L));

         // Convert the new HSL color back to RGB and add it to the list
         variations[i] = HslToRgb(newHsl);
      }

      return variations;
   }

   #region HSL / RGB Conversion Helpers

   private static HslColor RgbToHsl(Color color)
   {
      var r = color.R / 255.0;
      var g = color.G / 255.0;
      var b = color.B / 255.0;

      var max = Math.Max(r, Math.Max(g, b));
      var min = Math.Min(r, Math.Min(g, b));
      double h = 0,
             s,
             l = (max + min) / 2;

      if (Math.Abs(max - min) < 0.001)
      {
         h = s = 0; // achromatic (gray)
      }
      else
      {
         var d = max - min;
         s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

         if (AreFloatsEqual(max, r))
            h = (g - b) / d + (g < b ? 6 : 0);
         else if (AreFloatsEqual(max, g))
            h = (b - r) / d + 2;
         else if (AreFloatsEqual(max, b))
            h = (r - g) / d + 4;

         h /= 6;
      }

      return new ()
      {
         H = h * 360,
         S = s,
         L = l,
      };
   }

   private static bool AreFloatsEqual(double a, double b, double epsilon = 0.001)
   {
      return Math.Abs(a - b) < epsilon;
   }

   private static Color HslToRgb(HslColor hsl)
   {
      double r,
             g,
             b;
      var h = hsl.H / 360.0;
      var s = hsl.S;
      var l = hsl.L;

      if (Math.Abs(s) < 0.001)
      {
         r = g = b = l; // achromatic
      }
      else
      {
         var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
         var p = 2 * l - q;

         r = HueToRgb(p, q, h + 1.0 / 3.0);
         g = HueToRgb(p, q, h);
         b = HueToRgb(p, q, h - 1.0 / 3.0);
      }

      return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
   }

   private static double HueToRgb(double p, double q, double t)
   {
      if (t < 0)
         t += 1;
      if (t > 1)
         t -= 1;
      return t switch
      {
         < 1.0 / 6.0 => p + (q - p) * 6 * t,
         < 1.0 / 2.0 => q,
         < 2.0 / 3.0 => p + (q - p) * (2.0 / 3.0 - t) * 6,
         _ => p,
      };
   }

   #endregion

   /// <summary>
   /// Generates a perceptually uniform color scale between two colors.
   /// Uses Oklab color space for interpolation.
   /// </summary>
   public static Color[] GenerateGradient(Color colorStart, Color colorEnd, int steps)
   {
      switch (steps)
      {
         case <= 0:
            return [];
         case 1:
            return [colorStart];
      }

      var results = new Color[steps];

      // Convert RGB to Oklab
      var start = RgbToOklab(colorStart);
      var end = RgbToOklab(colorEnd);

      // Interpolate
      for (var i = 0; i < steps; i++)
      {
         // t is the percentage (0.0 to 1.0)
         var t = (float)i / (steps - 1);

         // Lerp (Linear Interpolation) in Oklab space
         var l = start.L + (end.L - start.L) * t;
         var a = start.A + (end.A - start.A) * t;
         var b = start.B + (end.B - start.B) * t;

         // Convert back to RGB
         results[i] = OklabToRgb(new (l, a, b));
      }

      return results;
   }

   private struct Oklab(float l, float a, float b)
   {
      public readonly float L = l; // Lightness
      public readonly float A = a; // Green-Red
      public readonly float B = b; // Blue-Yellow
   }

   private static Oklab RgbToOklab(Color c)
   {
      // Convert 0-255 to 0-1 Linear RGB (Gamma Correction)
      var r = InverseGamma(c.R / 255.0f);
      var g = InverseGamma(c.G / 255.0f);
      var b = InverseGamma(c.B / 255.0f);

      // Linear RGB to LMS (Matrix multiplication)
      var l = 0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b;
      var m = 0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b;
      var s = 0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b;

      // Cube root of LMS
      var l_ = MathF.Cbrt(l);
      var m_ = MathF.Cbrt(m);
      var s_ = MathF.Cbrt(s);

      // LMS to Oklab
      return new (0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_,
                  1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_,
                  0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_);
   }

   private static Color OklabToRgb(Oklab oklab)
   {
      var l_ = oklab.L + 0.3963377774f * oklab.A + 0.2158037573f * oklab.B;
      var m_ = oklab.L - 0.1055613458f * oklab.A - 0.0638541728f * oklab.B;
      var s_ = oklab.L - 0.0894841775f * oklab.A - 1.2914855480f * oklab.B;

      var l = l_ * l_ * l_;
      var m = m_ * m_ * m_;
      var s = s_ * s_ * s_;

      var r = 4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
      var g = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
      var b = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;

      // Apply Gamma to get back to sRGB and clamp to 0-255
      return Color.FromArgb(255,
                            (byte)ClampToByte(ApplyGamma(r) * 255.0f),
                            (byte)ClampToByte(ApplyGamma(g) * 255.0f),
                            (byte)ClampToByte(ApplyGamma(b) * 255.0f));
   }

   // Convert sRGB to Linear RGB
   private static float InverseGamma(float c)
   {
      return c >= 0.04045f ? MathF.Pow((c + 0.055f) / 1.055f, 2.4f) : c / 12.92f;
   }

   // Convert Linear RGB to sRGB
   private static float ApplyGamma(float c)
   {
      return c >= 0.0031308f ? 1.055f * MathF.Pow(c, 1.0f / 2.4f) - 0.055f : 12.92f * c;
   }

   private static int ClampToByte(float f)
   {
      return f switch
      {
         < 0 => 0,
         > 255 => 255,
         _ => (int)MathF.Round(f)
      };
   }
}