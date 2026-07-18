package com.yourapp.recipes.presentation.theme

import android.os.Build
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext

private val LightColorScheme = lightColorScheme(
    primary = Color(0xFFE65100),
    onPrimary = Color.White,
    primaryContainer = Color(0xFFFFDBC8),
    onPrimaryContainer = Color(0xFF331200),
    secondary = Color(0xFF775A00),
    onSecondary = Color.White,
    secondaryContainer = Color(0xFFFFDF9A),
    onSecondaryContainer = Color(0xFF261A00),
    tertiary = Color(0xFF52643F),
    onTertiary = Color.White,
    tertiaryContainer = Color(0xFFD5EABB),
    onTertiaryContainer = Color(0xFF111F01),
    error = Color(0xFFBA1A1A),
    background = Color(0xFFFFFBFF),
    surface = Color(0xFFFFFBFF)
)

private val DarkColorScheme = darkColorScheme(
    primary = Color(0xFFFFB68E),
    onPrimary = Color(0xFF552200),
    primaryContainer = Color(0xFF793800),
    onPrimaryContainer = Color(0xFFFFDBC8),
    secondary = Color(0xFFEBC348),
    onSecondary = Color(0xFF3E2E00),
    secondaryContainer = Color(0xFF5A4300),
    onSecondaryContainer = Color(0xFFFFDF9A),
    tertiary = Color(0xFFB9CEA0),
    onTertiary = Color(0xFF253515),
    tertiaryContainer = Color(0xFF3B4C29),
    onTertiaryContainer = Color(0xFFD5EABB),
    error = Color(0xFFFFB4AB),
    background = Color(0xFF1C1B1F),
    surface = Color(0xFF1C1B1F)
)

@Composable
fun RecipesTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    dynamicColor: Boolean = true,
    content: @Composable () -> Unit
) {
    val colorScheme = when {
        dynamicColor && Build.VERSION.SDK_INT >= Build.VERSION_CODES.S -> {
            val context = LocalContext.current
            if (darkTheme) dynamicDarkColorScheme(context) 
            else dynamicLightColorScheme(context)
        }
        darkTheme -> DarkColorScheme
        else -> LightColorScheme
    }

    MaterialTheme(
        colorScheme = colorScheme,
        typography = Typography(),
        content = content
    )
}
