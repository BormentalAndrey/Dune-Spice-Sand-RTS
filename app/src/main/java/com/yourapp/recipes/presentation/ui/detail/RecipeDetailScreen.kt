package com.yourapp.recipes.presentation.ui.detail

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
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
        recipe?.let { rec ->
            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(padding),
                contentPadding = PaddingValues(16.dp),
                verticalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                // Название и описание
                item {
                    Text(
                        text = rec.title,
                        style = MaterialTheme.typography.headlineMedium
                    )
                }
                
                item {
                    Text(
                        text = rec.description,
                        style = MaterialTheme.typography.bodyLarge,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
                
                // Время и сложность
                item {
                    Row(
                        horizontalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        AssistChip(
                            onClick = {},
                            label = { Text("⏱ ${rec.cookingTimeMinutes} мин") }
                        )
                        AssistChip(
                            onClick = {},
                            label = { Text("📊 ${rec.difficulty.displayName}") }
                        )
                        AssistChip(
                            onClick = {},
                            label = { Text("🍽 ${rec.servings} порций") }
                        )
                    }
                }
                
                // Умножитель порций
                item {
                    Card(
                        colors = CardDefaults.cardColors(
                            containerColor = MaterialTheme.colorScheme.surfaceVariant
                        )
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            horizontalArrangement = Arrangement.Center,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            IconButton(onClick = { 
                                if (servingsMultiplier > 1) 
                                    viewModel.updateServingsMultiplier(servingsMultiplier - 1) 
                            }) {
                                Icon(Icons.Default.Remove, "Уменьшить")
                            }
                            Text(
                                "${servingsMultiplier}x порций (${rec.servings * servingsMultiplier} персон)",
                                style = MaterialTheme.typography.titleMedium
                            )
                            IconButton(onClick = { 
                                viewModel.updateServingsMultiplier(servingsMultiplier + 1) 
                            }) {
                                Icon(Icons.Default.Add, "Увеличить")
                            }
                        }
                    }
                }
                
                // КБЖУ
                item {
                    Card {
                        Column(modifier = Modifier.padding(16.dp)) {
                            Text("Пищевая ценность на порцию:", fontWeight = FontWeight.Bold)
                            Spacer(modifier = Modifier.height(8.dp))
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceEvenly
                            ) {
                                NutritionItem("Калории", "${adjustedNutrition?.calories?.toInt() ?: rec.nutritionInfo.calories.toInt()}", "ккал")
                                NutritionItem("Белки", "${adjustedNutrition?.proteins?.toInt() ?: rec.nutritionInfo.proteins.toInt()}", "г")
                                NutritionItem("Жиры", "${adjustedNutrition?.fats?.toInt() ?: rec.nutritionInfo.fats.toInt()}", "г")
                                NutritionItem("Углеводы", "${adjustedNutrition?.carbohydrates?.toInt() ?: rec.nutritionInfo.carbohydrates.toInt()}", "г")
                            }
                        }
                    }
                }
                
                // Ингредиенты
                item {
                    Text(
                        "Ингредиенты:",
                        style = MaterialTheme.typography.titleLarge,
                        fontWeight = FontWeight.Bold
                    )
                }
                
                itemsIndexed(rec.ingredients) { index, ingredient ->
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(
                            containerColor = if (index % 2 == 0) 
                                MaterialTheme.colorScheme.surface 
                            else 
                                MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)
                        )
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Text(
                                text = ingredient.name,
                                style = MaterialTheme.typography.bodyLarge
                            )
                            Text(
                                text = "${(ingredient.quantity * servingsMultiplier).toInt()} ${ingredient.unit}",
                                style = MaterialTheme.typography.bodyLarge,
                                color = MaterialTheme.colorScheme.primary
                            )
                        }
                    }
                }
                
                // Шаги приготовления
                item {
                    Text(
                        "Приготовление:",
                        style = MaterialTheme.typography.titleLarge,
                        fontWeight = FontWeight.Bold
                    )
                }
                
                // Прогресс
                item {
                    LinearProgressIndicator(
                        progress = cookingProgress,
                        modifier = Modifier.fillMaxWidth()
                    )
                    Text(
                        "Выполнено: ${completedSteps.size} из ${rec.steps.size}",
                        style = MaterialTheme.typography.bodySmall
                    )
                }
                
                itemsIndexed(rec.steps) { index, step ->
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(
                            containerColor = if (completedSteps.contains(step.stepNumber))
                                MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.3f)
                            else
                                MaterialTheme.colorScheme.surface
                        )
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Checkbox(
                                checked = completedSteps.contains(step.stepNumber),
                                onCheckedChange = { 
                                    viewModel.toggleStepCompletion(step.stepNumber) 
                                }
                            )
                            Spacer(modifier = Modifier.width(8.dp))
                            Column(modifier = Modifier.weight(1f)) {
                                Text(
                                    text = "Шаг ${step.stepNumber}",
                                    style = MaterialTheme.typography.titleSmall,
                                    color = MaterialTheme.colorScheme.primary
                                )
                                Text(
                                    text = step.description,
                                    style = MaterialTheme.typography.bodyMedium
                                )
                                if (step.durationMinutes > 0) {
                                    Text(
                                        text = "⏱ ${step.durationMinutes} мин",
                                        style = MaterialTheme.typography.bodySmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                }
                            }
                        }
                    }
                }
            }
        } ?: run {
            // Если рецепт не загружен
            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(padding),
                contentAlignment = Alignment.Center
            ) {
                CircularProgressIndicator()
            }
        }
    }
}

@Composable
fun NutritionItem(label: String, value: String, unit: String) {
    Column(horizontalAlignment = Alignment.CenterHorizontally) {
        Text(
            text = value,
            style = MaterialTheme.typography.titleLarge,
            color = MaterialTheme.colorScheme.primary
        )
        Text(
            text = unit,
            style = MaterialTheme.typography.bodySmall
        )
        Text(
            text = label,
            style = MaterialTheme.typography.bodySmall
        )
    }
}
