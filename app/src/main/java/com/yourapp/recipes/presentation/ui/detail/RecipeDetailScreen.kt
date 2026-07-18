package com.yourapp.recipes.presentation.ui.detail

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import com.yourapp.recipes.presentation.viewmodel.RecipeDetailViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun RecipeDetailScreen(
    recipeId: Long,
    onEditClick: () -> Unit,
    onBackClick: () -> Unit,
    viewModel: RecipeDetailViewModel = hiltViewModel()
) {
    val recipe by viewModel.recipe.collectAsState()
    val servingsMultiplier by viewModel.servingsMultiplier.collectAsState()
    val completedSteps by viewModel.completedSteps.collectAsState()
    val cookingProgress by viewModel.cookingProgress.collectAsState()
    val adjustedNutrition by viewModel.adjustedNutrition.collectAsState()
    
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(recipe?.title ?: "Загрузка...") },
                navigationIcon = {
                    IconButton(onClick = onBackClick) {
                        Icon(Icons.Default.ArrowBack, "Назад")
                    }
                },
                actions = {
                    IconButton(onClick = onEditClick) {
                        Icon(Icons.Default.Edit, "Редактировать")
                    }
                    IconButton(onClick = { viewModel.toggleFavorite() }) {
                        Icon(
                            if (recipe?.isFavorite == true) Icons.Default.Favorite 
                            else Icons.Default.FavoriteBorder,
                            "Избранное"
                        )
                    }
                }
            )
        }
    ) { padding ->
        LazyColumn(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding),
            contentPadding = PaddingValues(16.dp)
        ) {
            // Здесь будет детальная информация о рецепте
            item {
                Text(
                    text = recipe?.title ?: "",
                    style = MaterialTheme.typography.headlineMedium
                )
            }
            
            // Умножитель порций
            item {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.Center
                ) {
                    IconButton(onClick = { 
                        if (servingsMultiplier > 1) 
                            viewModel.updateServingsMultiplier(servingsMultiplier - 1) 
                    }) {
                        Icon(Icons.Default.Remove, "Уменьшить")
                    }
                    Text("${servingsMultiplier}x порций")
                    IconButton(onClick = { 
                        viewModel.updateServingsMultiplier(servingsMultiplier + 1) 
                    }) {
                        Icon(Icons.Default.Add, "Увеличить")
                    }
                }
            }
            
            // Прогресс приготовления
            item {
                LinearProgressIndicator(
                    progress = cookingProgress,
                    modifier = Modifier.fillMaxWidth()
                )
            }
        }
    }
}
