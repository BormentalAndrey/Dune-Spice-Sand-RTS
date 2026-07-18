package com.yourapp.recipes.presentation.ui.edit

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import com.yourapp.recipes.domain.model.*
import com.yourapp.recipes.presentation.ui.components.PhotoPicker
import com.yourapp.recipes.presentation.viewmodel.RecipeDetailViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun RecipeEditScreen(
    recipeId: Long,
    onSaveClick: () -> Unit,
    onBackClick: () -> Unit,
    viewModel: RecipeDetailViewModel = hiltViewModel()
) {
    val recipe by viewModel.recipe.collectAsState()
    var title by remember(recipe) { mutableStateOf(recipe?.title ?: "") }
    var description by remember(recipe) { mutableStateOf(recipe?.description ?: "") }
    var cookingTime by remember(recipe) { mutableStateOf(recipe?.cookingTimeMinutes?.toString() ?: "") }
    var selectedCategory by remember(recipe) { mutableStateOf(recipe?.category ?: Category.DINNER) }
    var selectedDifficulty by remember(recipe) { mutableStateOf(recipe?.difficulty ?: Difficulty.EASY) }
    var photoPath by remember(recipe) { mutableStateOf(recipe?.photoPath) }
    
    var categoryExpanded by remember { mutableStateOf(false) }
    var difficultyExpanded by remember { mutableStateOf(false) }
    
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(if (recipeId == 0L) "Новый рецепт" else "Редактирование") },
                navigationIcon = {
                    IconButton(onClick = onBackClick) {
                        Icon(Icons.Default.ArrowBack, "Назад")
                    }
                },
                actions = {
                    TextButton(
                        onClick = {
                            val updatedRecipe = recipe?.copy(
                                title = title,
                                description = description,
                                cookingTimeMinutes = cookingTime.toIntOrNull() ?: 0,
                                category = selectedCategory,
                                difficulty = selectedDifficulty,
                                photoPath = photoPath
                            ) ?: Recipe(
                                title = title,
                                description = description,
                                cookingTimeMinutes = cookingTime.toIntOrNull() ?: 0,
                                category = selectedCategory,
                                difficulty = selectedDifficulty,
                                photoPath = photoPath
                            )
                            viewModel.saveRecipe(updatedRecipe)
                            onSaveClick()
                        },
                        enabled = title.isNotBlank()
                    ) {
                        Text("Сохранить")
                    }
                }
            )
        }
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .verticalScroll(rememberScrollState())
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            // Фото
            PhotoPicker(
                photoPath = photoPath,
                onPhotoSelected = { photoPath = it }
            )
            
            // Название
            OutlinedTextField(
                value = title,
                onValueChange = { title = it },
                label = { Text("Название рецепта") },
                modifier = Modifier.fillMaxWidth()
            )
            
            // Описание
            OutlinedTextField(
                value = description,
                onValueChange = { description = it },
                label = { Text("Описание") },
                modifier = Modifier.fillMaxWidth(),
                minLines = 3
            )
            
            // Время приготовления
            OutlinedTextField(
                value = cookingTime,
                onValueChange = { cookingTime = it },
                label = { Text("Время приготовления (мин)") },
                modifier = Modifier.fillMaxWidth(),
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
            )
            
            // Категория
            ExposedDropdownMenuBox(
                expanded = categoryExpanded,
                onExpandedChange = { categoryExpanged = it }
            ) {
                OutlinedTextField(
                    value = selectedCategory.displayName,
                    onValueChange = {},
                    readOnly = true,
                    label = { Text("Категория") },
                    trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(expanded = categoryExpanded) },
                    modifier = Modifier.menuAnchor().fillMaxWidth()
                )
                ExposedDropdownMenu(
                    expanded = categoryExpanded,
                    onDismissRequest = { categoryExpanded = false }
                ) {
                    Category.values().forEach { category ->
                        DropdownMenuItem(
                            text = { Text(category.displayName) },
                            onClick = {
                                selectedCategory = category
                                categoryExpanded = false
                            }
                        )
                    }
                }
            }
            
            // Сложность
            ExposedDropdownMenuBox(
                expanded = difficultyExpanded,
                onExpandedChange = { difficultyExpanded = it }
            ) {
                OutlinedTextField(
                    value = selectedDifficulty.displayName,
                    onValueChange = {},
                    readOnly = true,
                    label = { Text("Сложность") },
                    trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(expanded = difficultyExpanded) },
                    modifier = Modifier.menuAnchor().fillMaxWidth()
                )
                ExposedDropdownMenu(
                    expanded = difficultyExpanded,
                    onDismissRequest = { difficultyExpanded = false }
                ) {
                    Difficulty.values().forEach { difficulty ->
                        DropdownMenuItem(
                            text = { Text(difficulty.displayName) },
                            onClick = {
                                selectedDifficulty = difficulty
                                difficultyExpanded = false
                            }
                        )
                    }
                }
            }
        }
    }
}
