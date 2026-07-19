package com.yourapp.recipes.presentation.ui.edit

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
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
    var servings by remember(recipe) { mutableStateOf(recipe?.servings?.toString() ?: "2") }
    var selectedCategory by remember(recipe) { mutableStateOf(recipe?.category ?: Category.DINNER) }
    var selectedDifficulty by remember(recipe) { mutableStateOf(recipe?.difficulty ?: Difficulty.EASY) }
    var photoPath by remember(recipe) { mutableStateOf(recipe?.photoPath) }
    
    // Ингредиенты
    var ingredients by remember(recipe) {
        mutableStateOf(recipe?.ingredients?.toMutableList() ?: mutableListOf())
    }
    
    // Шаги
    var steps by remember(recipe) {
        mutableStateOf(recipe?.steps?.toMutableList() ?: mutableListOf())
    }
    
    // КБЖУ
    var calories by remember(recipe) { mutableStateOf(recipe?.nutritionInfo?.calories?.toString() ?: "") }
    var proteins by remember(recipe) { mutableStateOf(recipe?.nutritionInfo?.proteins?.toString() ?: "") }
    var fats by remember(recipe) { mutableStateOf(recipe?.nutritionInfo?.fats?.toString() ?: "") }
    var carbohydrates by remember(recipe) { mutableStateOf(recipe?.nutritionInfo?.carbohydrates?.toString() ?: "") }
    
    var categoryExpanded by remember { mutableStateOf(false) }
    var difficultyExpanded by remember { mutableStateOf(false) }
    var showIngredientDialog by remember { mutableStateOf(false) }
    var showStepDialog by remember { mutableStateOf(false) }
    var editingIngredientIndex by remember { mutableStateOf(-1) }
    var editingStepIndex by remember { mutableStateOf(-1) }
    
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
                            val updatedRecipe = Recipe(
                                id = recipe?.id ?: 0,
                                title = title,
                                description = description,
                                cookingTimeMinutes = cookingTime.toIntOrNull() ?: 0,
                                category = selectedCategory,
                                difficulty = selectedDifficulty,
                                ingredients = ingredients,
                                steps = steps,
                                nutritionInfo = NutritionInfo(
                                    calories = calories.toFloatOrNull() ?: 0f,
                                    proteins = proteins.toFloatOrNull() ?: 0f,
                                    fats = fats.toFloatOrNull() ?: 0f,
                                    carbohydrates = carbohydrates.toFloatOrNull() ?: 0f
                                ),
                                servings = servings.toIntOrNull() ?: 2,
                                photoPath = photoPath,
                                isFavorite = recipe?.isFavorite ?: false
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
            
            // Основная информация
            Text("Основная информация", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleMedium)
            
            OutlinedTextField(
                value = title,
                onValueChange = { title = it },
                label = { Text("Название рецепта *") },
                modifier = Modifier.fillMaxWidth(),
                singleLine = true
            )
            
            OutlinedTextField(
                value = description,
                onValueChange = { description = it },
                label = { Text("Описание") },
                modifier = Modifier.fillMaxWidth(),
                minLines = 3
            )
            
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                OutlinedTextField(
                    value = cookingTime,
                    onValueChange = { cookingTime = it },
                    label = { Text("Время (мин)") },
                    modifier = Modifier.weight(1f),
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true
                )
                
                OutlinedTextField(
                    value = servings,
                    onValueChange = { servings = it },
                    label = { Text("Порций") },
                    modifier = Modifier.weight(1f),
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true
                )
            }
            
            // Категория
            ExposedDropdownMenuBox(
                expanded = categoryExpanded,
                onExpandedChange = { categoryExpanded = it }
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
                            text = { Text("${category.icon} ${category.displayName}") },
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
            
            Divider()
            
            // Ингредиенты
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text("Ингредиенты", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleMedium)
                TextButton(onClick = {
                    editingIngredientIndex = -1
                    showIngredientDialog = true
                }) {
                    Icon(Icons.Default.Add, null, Modifier.size(18.dp))
                    Spacer(Modifier.width(4.dp))
                    Text("Добавить")
                }
            }
            
            if (ingredients.isEmpty()) {
                Text(
                    "Нет ингредиентов. Нажмите \"Добавить\"",
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    style = MaterialTheme.typography.bodyMedium
                )
            } else {
                ingredients.forEachIndexed { index, ingredient ->
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(
                            containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)
                        )
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Column(modifier = Modifier.weight(1f)) {
                                Text(ingredient.name, fontWeight = FontWeight.Medium)
                                Text(
                                    "${ingredient.quantity} ${ingredient.unit}",
                                    style = MaterialTheme.typography.bodySmall,
                                    color = MaterialTheme.colorScheme.onSurfaceVariant
                                )
                            }
                            IconButton(onClick = {
                                editingIngredientIndex = index
                                showIngredientDialog = true
                            }) {
                                Icon(Icons.Default.Edit, "Редактировать", Modifier.size(18.dp))
                            }
                            IconButton(onClick = {
                                ingredients = ingredients.toMutableList().also { it.removeAt(index) }
                            }) {
                                Icon(Icons.Default.Delete, "Удалить", Modifier.size(18.dp), tint = MaterialTheme.colorScheme.error)
                            }
                        }
                    }
                }
            }
            
            Divider()
            
            // Шаги приготовления
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text("Шаги приготовления", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleMedium)
                TextButton(onClick = {
                    editingStepIndex = -1
                    showStepDialog = true
                }) {
                    Icon(Icons.Default.Add, null, Modifier.size(18.dp))
                    Spacer(Modifier.width(4.dp))
                    Text("Добавить")
                }
            }
            
            if (steps.isEmpty()) {
                Text(
                    "Нет шагов. Нажмите \"Добавить\"",
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    style = MaterialTheme.typography.bodyMedium
                )
            } else {
                steps.forEachIndexed { index, step ->
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(
                            containerColor = MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)
                        )
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            verticalAlignment = Alignment.Top
                        ) {
                            Text(
                                "Шаг ${step.stepNumber}:",
                                fontWeight = FontWeight.Bold,
                                modifier = Modifier.width(60.dp)
                            )
                            Column(modifier = Modifier.weight(1f)) {
                                Text(step.description)
                                if (step.durationMinutes > 0) {
                                    Text(
                                        "⏱ ${step.durationMinutes} мин",
                                        style = MaterialTheme.typography.bodySmall,
                                        color = MaterialTheme.colorScheme.primary
                                    )
                                }
                            }
                            IconButton(onClick = {
                                editingStepIndex = index
                                showStepDialog = true
                            }) {
                                Icon(Icons.Default.Edit, "Редактировать", Modifier.size(18.dp))
                            }
                            IconButton(onClick = {
                                steps = steps.toMutableList().also { it.removeAt(index) }
                            }) {
                                Icon(Icons.Default.Delete, "Удалить", Modifier.size(18.dp), tint = MaterialTheme.colorScheme.error)
                            }
                        }
                    }
                }
            }
            
            Divider()
            
            // КБЖУ
            Text("Пищевая ценность (на порцию)", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleMedium)
            
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                OutlinedTextField(
                    value = calories,
                    onValueChange = { calories = it },
                    label = { Text("Ккал") },
                    modifier = Modifier.weight(1f),
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true
                )
                OutlinedTextField(
                    value = proteins,
                    onValueChange = { proteins = it },
                    label = { Text("Белки (г)") },
                    modifier = Modifier.weight(1f),
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true
                )
            }
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                OutlinedTextField(
                    value = fats,
                    onValueChange = { fats = it },
                    label = { Text("Жиры (г)") },
                    modifier = Modifier.weight(1f),
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true
                )
                OutlinedTextField(
                    value = carbohydrates,
                    onValueChange = { carbohydrates = it },
                    label = { Text("Углеводы (г)") },
                    modifier = Modifier.weight(1f),
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true
                )
            }
            
            Spacer(modifier = Modifier.height(32.dp))
        }
    }
    
    // Диалог добавления/редактирования ингредиента
    if (showIngredientDialog) {
        IngredientDialog(
            ingredient = if (editingIngredientIndex >= 0) ingredients.getOrNull(editingIngredientIndex) else null,
            onDismiss = { showIngredientDialog = false },
            onSave = { ingredient ->
                if (editingIngredientIndex >= 0) {
                    ingredients = ingredients.toMutableList().also {
                        it[editingIngredientIndex] = ingredient
                    }
                } else {
                    ingredients = ingredients + ingredient
                }
                showIngredientDialog = false
            }
        )
    }
    
    // Диалог добавления/редактирования шага
    if (showStepDialog) {
        StepDialog(
            step = if (editingStepIndex >= 0) steps.getOrNull(editingStepIndex) else null,
            stepNumber = if (editingStepIndex >= 0) (editingStepIndex + 1) else steps.size + 1,
            onDismiss = { showStepDialog = false },
            onSave = { step ->
                if (editingStepIndex >= 0) {
                    steps = steps.toMutableList().also {
                        it[editingStepIndex] = step
                    }
                } else {
                    steps = steps + step
                }
                showStepDialog = false
            }
        )
    }
}

@Composable
fun IngredientDialog(
    ingredient: Ingredient?,
    onDismiss: () -> Unit,
    onSave: (Ingredient) -> Unit
) {
    var name by remember { mutableStateOf(ingredient?.name ?: "") }
    var quantity by remember { mutableStateOf(ingredient?.quantity?.toString() ?: "") }
    var unit by remember { mutableStateOf(ingredient?.unit ?: "г") }
    
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text(if (ingredient != null) "Редактировать ингредиент" else "Добавить ингредиент") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                OutlinedTextField(
                    value = name,
                    onValueChange = { name = it },
                    label = { Text("Название") },
                    modifier = Modifier.fillMaxWidth(),
                    singleLine = true
                )
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    OutlinedTextField(
                        value = quantity,
                        onValueChange = { quantity = it },
                        label = { Text("Количество") },
                        modifier = Modifier.weight(1f),
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        singleLine = true
                    )
                    OutlinedTextField(
                        value = unit,
                        onValueChange = { unit = it },
                        label = { Text("Ед. изм.") },
                        modifier = Modifier.weight(1f),
                        singleLine = true
                    )
                }
            }
        },
        confirmButton = {
            TextButton(onClick = {
                onSave(Ingredient(
                    id = ingredient?.id ?: java.util.UUID.randomUUID().toString(),
                    name = name,
                    quantity = quantity.toFloatOrNull() ?: 0f,
                    unit = unit
                ))
            }, enabled = name.isNotBlank()) {
                Text("Сохранить")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) { Text("Отмена") }
        }
    )
}

@Composable
fun StepDialog(
    step: CookingStep?,
    stepNumber: Int,
    onDismiss: () -> Unit,
    onSave: (CookingStep) -> Unit
) {
    var description by remember { mutableStateOf(step?.description ?: "") }
    var duration by remember { mutableStateOf(step?.durationMinutes?.toString() ?: "") }
    
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text(if (step != null) "Редактировать шаг $stepNumber" else "Добавить шаг $stepNumber") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                OutlinedTextField(
                    value = description,
                    onValueChange = { description = it },
                    label = { Text("Описание шага") },
                    modifier = Modifier.fillMaxWidth(),
                    minLines = 2
                )
                OutlinedTextField(
                    value = duration,
                    onValueChange = { duration = it },
                    label = { Text("Время (мин)") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true
                )
            }
        },
        confirmButton = {
            TextButton(onClick = {
                onSave(CookingStep(
                    stepNumber = stepNumber,
                    description = description,
                    durationMinutes = duration.toIntOrNull() ?: 0
                ))
            }, enabled = description.isNotBlank()) {
                Text("Сохранить")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) { Text("Отмена") }
        }
    )
}
