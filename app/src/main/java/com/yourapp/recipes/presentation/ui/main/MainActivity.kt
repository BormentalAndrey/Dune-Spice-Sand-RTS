package com.yourapp.recipes.presentation.ui.main

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.ui.Modifier
import com.yourapp.recipes.presentation.theme.RecipesTheme
import com.yourapp.recipes.presentation.ui.navigation.RecipesNavGraph
import dagger.hilt.android.AndroidEntryPoint

/**
 * Главная Activity приложения.
 * Использует single-activity архитектуру с навигацией через Compose Navigation.
 */
@AndroidEntryPoint
class MainActivity : ComponentActivity() {
    
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        
        setContent {
            RecipesTheme {
                Surface(
                    modifier = Modifier.fillMaxSize(),
                    color = MaterialTheme.colorScheme.background
                ) {
                    RecipesNavGraph()
                }
            }
        }
    }
}
