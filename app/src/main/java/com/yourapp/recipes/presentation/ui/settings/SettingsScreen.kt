package com.yourapp.recipes.presentation.ui.settings

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import com.yourapp.recipes.domain.model.MeasurementSystem
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
    val context = LocalContext.current
    
    var showThemeDialog by remember { mutableStateOf(false) }
    var showMeasurementDialog by remember { mutableStateOf(false) }
    var showExportDialog by remember { mutableStateOf(false) }
    var showImportDialog by remember { mutableStateOf(false) }
    var showAboutDialog by remember { mutableStateOf(false) }
    var showClearDataDialog by remember { mutableStateOf(false) }
    
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
                .verticalScroll(rememberScrollState())
        ) {
            // Внешний вид
            Text(
                text = "Внешний вид",
                style = MaterialTheme.typography.titleSmall,
                color = MaterialTheme.colorScheme.primary,
                fontWeight = FontWeight.Bold,
                modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp)
            )
            
            ListItem(
                headlineContent = { Text("Тема оформления") },
                supportingContent = { 
                    Text(
                        when (preferences.themeMode) {
                            ThemeMode.LIGHT -> "Светлая"
                            ThemeMode.DARK -> "Темная"
                            ThemeMode.SYSTEM -> "Системная"
                        }
                    )
                },
                leadingContent = { Icon(Icons.Default.Palette, null) },
                trailingContent = { Icon(Icons.Default.ChevronRight, null) },
                modifier = Modifier.clickable { showThemeDialog = true }
            )
            
            Divider(modifier = Modifier.padding(horizontal = 16.dp))
            
            ListItem(
                headlineContent = { Text("Единицы измерения") },
                supportingContent = { Text(preferences.measurementSystem.displayName) },
                leadingContent = { Icon(Icons.Default.Straighten, null) },
                trailingContent = { Icon(Icons.Default.ChevronRight, null) },
                modifier = Modifier.clickable { showMeasurementDialog = true }
            )
            
            Spacer(modifier = Modifier.height(16.dp))
            
            // Данные
            Text(
                text = "Данные",
                style = MaterialTheme.typography.titleSmall,
                color = MaterialTheme.colorScheme.primary,
                fontWeight = FontWeight.Bold,
                modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp)
            )
            
            ListItem(
                headlineContent = { Text("Рецептов в базе") },
                supportingContent = { Text("$recipeCount") },
                leadingContent = { Icon(Icons.Default.MenuBook, null) }
            )
            
            Divider(modifier = Modifier.padding(horizontal = 16.dp))
            
            ListItem(
                headlineContent = { Text("Экспорт рецептов") },
                supportingContent = { Text("Сохранить все рецепты в JSON файл") },
                leadingContent = { Icon(Icons.Default.Upload, null) },
                modifier = Modifier.clickable { showExportDialog = true }
            )
            
            Divider(modifier = Modifier.padding(horizontal = 16.dp))
            
            ListItem(
                headlineContent = { Text("Импорт рецептов") },
                supportingContent = { Text("Восстановить рецепты из JSON файла") },
                leadingContent = { Icon(Icons.Default.Download, null) },
                modifier = Modifier.clickable { showImportDialog = true }
            )
            
            Divider(modifier = Modifier.padding(horizontal = 16.dp))
            
            ListItem(
                headlineContent = { Text("Очистить все данные") },
                supportingContent = { Text("Удалить все рецепты и настройки") },
                leadingContent = { 
                    Icon(Icons.Default.DeleteForever, null, tint = MaterialTheme.colorScheme.error) 
                },
                modifier = Modifier.clickable { showClearDataDialog = true }
            )
            
            Spacer(modifier = Modifier.height(16.dp))
            
            // Уведомления
            Text(
                text = "Уведомления",
                style = MaterialTheme.typography.titleSmall,
                color = MaterialTheme.colorScheme.primary,
                fontWeight = FontWeight.Bold,
                modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp)
            )
            
            ListItem(
                headlineContent = { Text("Напоминание о покупках") },
                supportingContent = { Text("Уведомление о некупленных продуктах") },
                leadingContent = { Icon(Icons.Default.Notifications, null) },
                trailingContent = {
                    Switch(
                        checked = preferences.shoppingListReminder,
                        onCheckedChange = { 
                            viewModel.savePreferences(
                                preferences.copy(shoppingListReminder = it)
                            )
                        }
                    )
                }
            )
            
            Spacer(modifier = Modifier.height(16.dp))
            
            // О приложении
            Text(
                text = "О приложении",
                style = MaterialTheme.typography.titleSmall,
                color = MaterialTheme.colorScheme.primary,
                fontWeight = FontWeight.Bold,
                modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp)
            )
            
            ListItem(
                headlineContent = { Text("О приложении") },
                supportingContent = { Text("Версия 1.0") },
                leadingContent = { Icon(Icons.Default.Info, null) },
                modifier = Modifier.clickable { showAboutDialog = true }
            )
            
            Divider(modifier = Modifier.padding(horizontal = 16.dp))
            
            ListItem(
                headlineContent = { Text("Политика конфиденциальности") },
                leadingContent = { Icon(Icons.Default.Security, null) },
                trailingContent = { Icon(Icons.Default.OpenInNew, null) }
            )
            
            Divider(modifier = Modifier.padding(horizontal = 16.dp))
            
            ListItem(
                headlineContent = { Text("Оценить приложение") },
                supportingContent = { Text("Поддержите нас в Google Play") },
                leadingContent = { Icon(Icons.Default.Star, null) }
            )
            
            Spacer(modifier = Modifier.height(32.dp))
        }
    }
    
    // Диалог выбора темы
    if (showThemeDialog) {
        AlertDialog(
            onDismissRequest = { showThemeDialog = false },
            title = { Text("Тема оформления") },
            text = {
                Column {
                    ThemeMode.values().forEach { mode ->
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .clickable {
                                    viewModel.updateTheme(mode.name.lowercase())
                                    showThemeDialog = false
                                }
                                .padding(vertical = 12.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            RadioButton(
                                selected = preferences.themeMode == mode,
                                onClick = {
                                    viewModel.updateTheme(mode.name.lowercase())
                                    showThemeDialog = false
                                }
                            )
                            Spacer(modifier = Modifier.width(12.dp))
                            Text(
                                when (mode) {
                                    ThemeMode.LIGHT -> "Светлая"
                                    ThemeMode.DARK -> "Темная"
                                    ThemeMode.SYSTEM -> "Системная (как на устройстве)"
                                }
                            )
                        }
                    }
                }
            },
            confirmButton = {
                TextButton(onClick = { showThemeDialog = false }) {
                    Text("Закрыть")
                }
            }
        )
    }
    
    // Диалог выбора единиц измерения
    if (showMeasurementDialog) {
        AlertDialog(
            onDismissRequest = { showMeasurementDialog = false },
            title = { Text("Единицы измерения") },
            text = {
                Column {
                    MeasurementSystem.values().forEach { system ->
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .clickable {
                                    viewModel.updateMeasurementSystem(system.name.lowercase())
                                    showMeasurementDialog = false
                                }
                                .padding(vertical = 12.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            RadioButton(
                                selected = preferences.measurementSystem == system,
                                onClick = {
                                    viewModel.updateMeasurementSystem(system.name.lowercase())
                                    showMeasurementDialog = false
                                }
                            )
                            Spacer(modifier = Modifier.width(12.dp))
                            Text(system.displayName)
                        }
                    }
                }
            },
            confirmButton = {
                TextButton(onClick = { showMeasurementDialog = false }) {
                    Text("Закрыть")
                }
            }
        )
    }
    
    // Диалог экспорта
    if (showExportDialog) {
        AlertDialog(
            onDismissRequest = { showExportDialog = false },
            title = { Text("Экспорт рецептов") },
            text = { 
                Text("Все рецепты будут сохранены в JSON файл. Вы можете перенести их на другое устройство или поделиться с друзьями.")
            },
            confirmButton = {
                TextButton(onClick = {
                    viewModel.exportRecipes()
                    showExportDialog = false
                }) {
                    Text("Экспортировать")
                }
            },
            dismissButton = {
                TextButton(onClick = { showExportDialog = false }) {
                    Text("Отмена")
                }
            }
        )
    }
    
    // Диалог импорта
    if (showImportDialog) {
        AlertDialog(
            onDismissRequest = { showImportDialog = false },
            title = { Text("Импорт рецептов") },
            text = { 
                Text("Выберите JSON файл с рецептами для восстановления. Существующие рецепты не будут удалены.")
            },
            confirmButton = {
                TextButton(onClick = {
                    viewModel.importRecipes("")
                    showImportDialog = false
                }) {
                    Text("Выбрать файл")
                }
            },
            dismissButton = {
                TextButton(onClick = { showImportDialog = false }) {
                    Text("Отмена")
                }
            }
        )
    }
    
    // Диалог очистки данных
    if (showClearDataDialog) {
        AlertDialog(
            onDismissRequest = { showClearDataDialog = false },
            title = { Text("Очистить все данные") },
            text = { 
                Text("Это действие удалит все рецепты, список покупок и план меню. Данное действие нельзя отменить.")
            },
            confirmButton = {
                TextButton(
                    onClick = {
                        showClearDataDialog = false
                    },
                    colors = ButtonDefaults.textButtonColors(
                        contentColor = MaterialTheme.colorScheme.error
                    )
                ) {
                    Text("Удалить все")
                }
            },
            dismissButton = {
                TextButton(onClick = { showClearDataDialog = false }) {
                    Text("Отмена")
                }
            }
        )
    }
    
    // Диалог "О приложении"
    if (showAboutDialog) {
        AlertDialog(
            onDismissRequest = { showAboutDialog = false },
            title = { Text("О приложении") },
            text = {
                Column {
                    Text("📱 Рецепты v1.0")
                    Spacer(modifier = Modifier.height(8.dp))
                    Text("Ваш персональный помощник в мире кулинарии. Создавайте, сохраняйте и делитесь любимыми рецептами.")
                    Spacer(modifier = Modifier.height(8.dp))
                    Text("Разработчик: Your Name")
                    Text("© 2024 Все права защищены")
                }
            },
            confirmButton = {
                TextButton(onClick = { showAboutDialog = false }) {
                    Text("OK")
                }
            }
        )
    }
}
