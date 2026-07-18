package com.yourapp.recipes.presentation.ui.planner

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import com.yourapp.recipes.presentation.viewmodel.MealPlannerViewModel
import com.yourapp.recipes.utils.toDayOfWeek
import com.yourapp.recipes.utils.toDisplayDate
import java.util.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MealPlannerScreen(
    onRecipeClick: (Long) -> Unit,
    onBackClick: () -> Unit,
    viewModel: MealPlannerViewModel = hiltViewModel()
) {
    val currentWeekStart by viewModel.currentWeekStart.collectAsState()
    val weekMealPlans by viewModel.weekMealPlans.collectAsState()
    val selectedDate by viewModel.selectedDate.collectAsState()
    
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
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
        ) {
            // Недельная навигация
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(16.dp),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                IconButton(onClick = { viewModel.previousWeek() }) {
                    Icon(Icons.Default.ChevronLeft, "Предыдущая неделя")
                }
                
                Text(
                    text = currentWeekStart.toDisplayDate(),
                    style = MaterialTheme.typography.titleMedium
                )
                
                IconButton(onClick = { viewModel.nextWeek() }) {
                    Icon(Icons.Default.ChevronRight, "Следующая неделя")
                }
            }
            
            // Дни недели
            LazyRow(
                modifier = Modifier.fillMaxWidth(),
                contentPadding = PaddingValues(horizontal = 16.dp),
                horizontalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                items(7) { dayOffset ->
                    val date = currentWeekStart + dayOffset * 24 * 60 * 60 * 1000L
                    val isSelected = date == selectedDate
                    
                    FilterChip(
                        selected = isSelected,
                        onClick = { viewModel.selectDate(date) },
                        label = {
                            Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                Text(date.toDayOfWeek())
                                Text(
                                    Calendar.getInstance().apply { 
                                        timeInMillis = date 
                                    }.get(Calendar.DAY_OF_MONTH).toString()
                                )
                            }
                        }
                    )
                }
            }
        }
    }
}
