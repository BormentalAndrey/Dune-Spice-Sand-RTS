package com.yourapp.recipes.presentation.ui.settings

import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import com.yourapp.recipes.domain.model.ThemeMode
import com.yourapp.recipes.presentation.viewmodel.SettingsViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SettingsScreen(
    onBackClick: () -> Unit,
    viewModel: SettingsViewModel = hiltViewModel()
) {
    val preferences by viewModel.preferences.collectAsState()
    val recipeCount by viewModel.recipeCount.collectAsState()
    
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Настройки") },
                navigationIcon = {
                    IconButton(onClick = onBackClick) {
                        Icon(Icons.Default.ArrowBack, "Назад")
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
            // Тема
            ListItem(
                headlineContent = { Text("Тема оформления") },
                supportingContent = { Text(preferences.themeMode.name) },
                leadingContent = {
                    Icon(Icons.Default.Palette, null)
                }
            )
            
            // Единицы измерения
            ListItem(
                headlineContent = { Text("Единицы измерения") },
                supportingContent = { Text(preferences.measurementSystem.displayName) },
                leadingContent = {
                    Icon(Icons.Default.Straighten, null)
                }
            )
            
            // Статистика
            ListItem(
                headlineContent = { Text("Рецептов в базе") },
                supportingContent = { Text("$recipeCount") },
                leadingContent = {
                    Icon(Icons.Default.MenuBook, null)
                }
            )
            
            Divider()
            
            // Экспорт/импорт
            ListItem(
                headlineContent = { Text("Экспорт рецептов") },
                supportingContent = { Text("Сохранить все рецепты в JSON") },
                leadingContent = {
                    Icon(Icons.Default.Upload, null)
                }
            )
            
            ListItem(
                headlineContent = { Text("Импорт рецептов") },
                supportingContent = { Text("Восстановить рецепты из JSON") },
                leadingContent = {
                    Icon(Icons.Default.Download, null)
                }
            )
            
            Divider()
            
            // О приложении
            ListItem(
                headlineContent = { Text("О приложении") },
                supportingContent = { Text("Версия 1.0") },
                leadingContent = {
                    Icon(Icons.Default.Info, null)
                }
            )
        }
    }
}
