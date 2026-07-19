package com.yourapp.recipes.presentation.ui.planner

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import com.yourapp.recipes.domain.model.MealType
import com.yourapp.recipes.presentation.viewmodel.MealPlannerViewModel
import com.yourapp.recipes.utils.toDayOfWeek
import com.yourapp.recipes.utils.toDisplayDate
import java.text.SimpleDateFormat
import java.util.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MealPlannerScreen(
    onRecipeClick: (Long) -> Unit,
    onBackClick: () -> Unit,
    viewModel: MealPlannerViewModel = hiltViewModel()
) {
    val currentWeekStart by viewModel.currentWeekStart.collectAsState()
    val selectedDate by viewModel.selectedDate.collectAsState()
    val dayMealPlans by viewModel.dayMealPlans.collectAsState()
    
    var showAddMealDialog by remember { mutableStateOf(false) }
    var selectedMealType by remember { mutableStateOf(MealType.DINNER) }
    
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("План меню") },
                navigationIcon = {
                    IconButton(onClick = onBackClick) {
                        Icon(Icons.Default.ArrowBack, "Назад")
                    }
                },
                actions = {
                    IconButton(onClick = { viewModel.generateShoppingList() }) {
                        Icon(Icons.Default.ShoppingCart, "Создать список покупок")
                    }
                }
            )
        }
    ) { padding ->
        Column(
            modifier = Modifier.fillMaxSize().padding(padding)
        ) {
            Card(
                modifier = Modifier.fillMaxWidth().padding(16.dp),
                colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant)
            ) {
                Row(
                    modifier = Modifier.fillMaxWidth().padding(12.dp),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    IconButton(onClick = { viewModel.previousWeek() }) {
                        Icon(Icons.Default.ChevronLeft, "Предыдущая неделя")
                    }
                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                        Text(getWeekRange(currentWeekStart), style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                        Text(currentWeekStart.toDisplayDate(), style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                    }
                    IconButton(onClick = { viewModel.nextWeek() }) {
                        Icon(Icons.Default.ChevronRight, "Следующая неделя")
                    }
                }
            }
            
            LazyRow(
                modifier = Modifier.fillMaxWidth(),
                contentPadding = PaddingValues(horizontal = 16.dp),
                horizontalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                items(7) { dayOffset ->
                    val date = currentWeekStart + dayOffset * 24 * 60 * 60 * 1000L
                    val isSelected = date == selectedDate
                    val isToday = date.isToday()
                    
                    DayChip(date = date, isSelected = isSelected, isToday = isToday, onClick = { viewModel.selectDate(date) })
                }
            }
            
            Spacer(Modifier.height(16.dp))
            
            Text("Приемы пищи на ${formatDate(selectedDate)}:", style = MaterialTheme.typography.titleMedium, modifier = Modifier.padding(horizontal = 16.dp))
            Spacer(Modifier.height(8.dp))
            
            LazyColumn(
                modifier = Modifier.fillMaxSize(),
                contentPadding = PaddingValues(16.dp),
                verticalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                val mealsForDay = dayMealPlans.groupBy { it.mealType }
                
                MealType.values().forEach { mealType ->
                    val meals = mealsForDay[mealType] ?: emptyList()
                    item {
                        MealSlotCard(
                            mealType = mealType,
                            meals = meals,
                            onAddClick = { selectedMealType = mealType; showAddMealDialog = true },
                            onRecipeClick = onRecipeClick,
                            onDeleteMeal = { viewModel.removeMealFromPlan(it) }
                        )
                    }
                }
            }
        }
    }
    
    if (showAddMealDialog) {
        AddMealDialog(
            mealType = selectedMealType,
            onDismiss = { showAddMealDialog = false },
            onAddMeal = { recipeId ->
                viewModel.addMealToPlan(recipeId, selectedDate, selectedMealType)
                showAddMealDialog = false
            },
            viewModel = viewModel
        )
    }
}

@Composable
fun DayChip(date: Long, isSelected: Boolean, isToday: Boolean, onClick: () -> Unit) {
    val calendar = Calendar.getInstance().apply { timeInMillis = date }
    val dayOfMonth = calendar.get(Calendar.DAY_OF_MONTH)
    val dayOfWeek = date.toDayOfWeek()
    
    Card(
        modifier = Modifier.width(70.dp).clickable(onClick = onClick),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(
            containerColor = when {
                isSelected -> MaterialTheme.colorScheme.primary
                isToday -> MaterialTheme.colorScheme.primaryContainer
                else -> MaterialTheme.colorScheme.surfaceVariant
            }
        ),
        border = if (isToday && !isSelected) BorderStroke(2.dp, MaterialTheme.colorScheme.primary) else null
    ) {
        Column(modifier = Modifier.padding(8.dp), horizontalAlignment = Alignment.CenterHorizontally) {
            Text(dayOfWeek, style = MaterialTheme.typography.labelMedium, color = if (isSelected) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.onSurface)
            Spacer(Modifier.height(4.dp))
            Text(dayOfMonth.toString(), style = MaterialTheme.typography.titleLarge, fontWeight = FontWeight.Bold, color = if (isSelected) MaterialTheme.colorScheme.onPrimary else MaterialTheme.colorScheme.onSurface)
        }
    }
}

@Composable
fun MealSlotCard(
    mealType: MealType,
    meals: List<com.yourapp.recipes.domain.model.MealPlan>,
    onAddClick: () -> Unit,
    onRecipeClick: (Long) -> Unit,
    onDeleteMeal: (Long) -> Unit
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surface),
        elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
    ) {
        Column(modifier = Modifier.padding(12.dp)) {
            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween, verticalAlignment = Alignment.CenterVertically) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Text(mealType.icon, style = MaterialTheme.typography.titleLarge)
                    Spacer(Modifier.width(8.dp))
                    Text(mealType.displayName, style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold)
                }
                TextButton(onClick = onAddClick) {
                    Icon(Icons.Default.Add, null, Modifier.size(16.dp))
                    Spacer(Modifier.width(4.dp))
                    Text("Добавить")
                }
            }
            
            if (meals.isEmpty()) {
                Text("Нет блюд", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant, modifier = Modifier.padding(vertical = 8.dp))
            } else {
                meals.forEach { meal ->
                    Divider(modifier = Modifier.padding(vertical = 4.dp))
                    Row(
                        modifier = Modifier.fillMaxWidth().clickable { meal.recipe?.id?.let { onRecipeClick(it) } }.padding(vertical = 4.dp),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Column(modifier = Modifier.weight(1f)) {
                            Text(meal.recipe?.title ?: "Блюдо", style = MaterialTheme.typography.bodyLarge, maxLines = 1, overflow = TextOverflow.Ellipsis)
                            if (meal.recipe != null) {
                                Text("${meal.recipe.cookingTimeMinutes} мин • ${meal.recipe.nutritionInfo.calories.toInt()} ккал", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                            }
                        }
                        IconButton(onClick = { onDeleteMeal(meal.id) }, modifier = Modifier.size(32.dp)) {
                            Icon(Icons.Default.Close, "Удалить", tint = MaterialTheme.colorScheme.error, modifier = Modifier.size(16.dp))
                        }
                    }
                }
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AddMealDialog(
    mealType: MealType,
    onDismiss: () -> Unit,
    onAddMeal: (Long) -> Unit,
    viewModel: MealPlannerViewModel
) {
    var searchQuery by remember { mutableStateOf("") }
    val allRecipes by viewModel.allRecipes.collectAsState()
    
    val filteredRecipes = remember(searchQuery, allRecipes) {
        if (searchQuery.isEmpty()) allRecipes
        else allRecipes.filter { it.title.contains(searchQuery, ignoreCase = true) || it.description.contains(searchQuery, ignoreCase = true) }
    }
    
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Добавить блюдо на ${mealType.displayName.lowercase()}") },
        text = {
            Column {
                OutlinedTextField(
                    value = searchQuery,
                    onValueChange = { searchQuery = it },
                    placeholder = { Text("Поиск рецепта...") },
                    modifier = Modifier.fillMaxWidth(),
                    singleLine = true,
                    leadingIcon = { Icon(Icons.Default.Search, null) }
                )
                Spacer(Modifier.height(8.dp))
                
                if (filteredRecipes.isEmpty()) {
                    Text("Рецепты не найдены", style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.onSurfaceVariant, modifier = Modifier.padding(16.dp))
                } else {
                    LazyColumn(modifier = Modifier.height(400.dp)) {
                        items(filteredRecipes.take(30)) { recipe ->
                            ListItem(
                                headlineContent = { Text(recipe.title, maxLines = 1) },
                                supportingContent = { Text("${recipe.cookingTimeMinutes} мин • ${recipe.category.displayName}") },
                                leadingContent = { Text(recipe.category.icon, style = MaterialTheme.typography.titleLarge) },
                                modifier = Modifier.clickable { onAddMeal(recipe.id) }
                            )
                            Divider()
                        }
                    }
                }
            }
        },
        confirmButton = { TextButton(onClick = onDismiss) { Text("Отмена") } }
    )
}

fun getWeekRange(startDate: Long): String {
    val endDate = startDate + 6 * 24 * 60 * 60 * 1000L
    val sdf = SimpleDateFormat("d MMMM", Locale("ru"))
    return "${sdf.format(Date(startDate))} - ${sdf.format(Date(endDate))}"
}

fun formatDate(timestamp: Long): String {
    val sdf = SimpleDateFormat("d MMMM, EEEE", Locale("ru"))
    return sdf.format(Date(timestamp))
}

fun Long.isToday(): Boolean {
    val today = Calendar.getInstance()
    val date = Calendar.getInstance().apply { timeInMillis = this@isToday }
    return today.get(Calendar.DAY_OF_YEAR) == date.get(Calendar.DAY_OF_YEAR) && today.get(Calendar.YEAR) == date.get(Calendar.YEAR)
}
