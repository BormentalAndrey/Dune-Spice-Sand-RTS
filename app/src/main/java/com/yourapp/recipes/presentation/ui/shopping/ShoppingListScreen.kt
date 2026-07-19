package com.yourapp.recipes.presentation.ui.shopping

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextDecoration
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import com.yourapp.recipes.domain.model.ShoppingItem
import com.yourapp.recipes.presentation.viewmodel.ShoppingListViewModel
import java.text.NumberFormat
import java.util.Locale

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ShoppingListScreen(
    onBackClick: () -> Unit,
    viewModel: ShoppingListViewModel = hiltViewModel()
) {
    val itemsGrouped by viewModel.itemsGroupedByCategory.collectAsState()
    val totalRemaining by viewModel.totalRemainingPrice.collectAsState()
    val totalSpentThisWeek by viewModel.totalSpentThisWeek.collectAsState()
    val spendingByCategory by viewModel.spendingByCategory.collectAsState()
    
    var showAddDialog by remember { mutableStateOf(false) }
    var showClearDialog by remember { mutableStateOf(false) }
    var showAnalytics by remember { mutableStateOf(false) }
    val context = LocalContext.current
    val numberFormat = remember { NumberFormat.getNumberInstance(Locale("ru")) }
    
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Список покупок") },
                navigationIcon = {
                    IconButton(onClick = onBackClick) {
                        Icon(Icons.Default.ArrowBack, "Назад")
                    }
                },
                actions = {
                    IconButton(onClick = { showAnalytics = !showAnalytics }) {
                        Icon(
                            if (showAnalytics) Icons.Default.PieChart else Icons.Default.PieChartOutline,
                            "Аналитика"
                        )
                    }
                    IconButton(onClick = { showAddDialog = true }) {
                        Icon(Icons.Default.Add, "Добавить")
                    }
                    IconButton(onClick = {
                        val text = viewModel.exportToText()
                        val sendIntent = android.content.Intent().apply {
                            action = android.content.Intent.ACTION_SEND
                            putExtra(android.content.Intent.EXTRA_TEXT, text)
                            type = "text/plain"
                        }
                        context.startActivity(android.content.Intent.createChooser(sendIntent, "Поделиться"))
                    }) {
                        Icon(Icons.Default.Share, "Поделиться")
                    }
                    IconButton(onClick = { showClearDialog = true }) {
                        Icon(Icons.Default.DeleteSweep, "Очистить")
                    }
                }
            )
        }
    ) { paddingValues ->
        if (itemsGrouped.isEmpty()) {
            Box(
                modifier = Modifier.fillMaxSize().padding(paddingValues),
                contentAlignment = Alignment.Center
            ) {
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    Icon(Icons.Default.ShoppingCart, null, Modifier.size(64.dp), tint = MaterialTheme.colorScheme.onSurfaceVariant)
                    Spacer(Modifier.height(16.dp))
                    Text("Список покупок пуст", style = MaterialTheme.typography.titleMedium)
                    Text("Добавьте ингредиенты из рецептов", style = MaterialTheme.typography.bodyMedium)
                }
            }
        } else {
            LazyColumn(
                modifier = Modifier.fillMaxSize().padding(paddingValues),
                contentPadding = PaddingValues(16.dp)
            ) {
                // Аналитика
                if (showAnalytics) {
                    item {
                        SpendingAnalyticsCard(
                            totalRemaining = totalRemaining,
                            totalSpentThisWeek = totalSpentThisWeek,
                            spendingByCategory = spendingByCategory,
                            numberFormat = numberFormat
                        )
                        Spacer(Modifier.height(16.dp))
                    }
                }
                
                // Итого
                item {
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.primaryContainer)
                    ) {
                        Column(modifier = Modifier.padding(16.dp)) {
                            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                Text("Осталось купить:", fontWeight = FontWeight.Bold)
                                Text(
                                    "${numberFormat.format(totalRemaining)} ₽",
                                    color = MaterialTheme.colorScheme.error,
                                    fontWeight = FontWeight.Bold
                                )
                            }
                            if (totalSpentThisWeek > 0) {
                                Spacer(Modifier.height(4.dp))
                                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                    Text("Потрачено за неделю:", fontWeight = FontWeight.Bold)
                                    Text("${numberFormat.format(totalSpentThisWeek)} ₽")
                                }
                            }
                        }
                    }
                    Spacer(Modifier.height(16.dp))
                }
                
                itemsGrouped.forEach { (category, items) ->
                    item { CategoryHeader(category = category, itemCount = items.size) }
                    items(items, key = { it.id }) { item ->
                        ShoppingListItem(
                            item = item,
                            onTogglePurchased = { isPurchased ->
                                viewModel.toggleItemPurchased(item.id, isPurchased)
                            },
                            onDelete = { viewModel.deleteItem(item.id) },
                            onPriceChange = { newPrice ->
                                viewModel.updateItemPrice(item.id, newPrice)
                            }
                        )
                    }
                    item { Spacer(Modifier.height(8.dp)) }
                }
            }
        }
    }
    
    if (showClearDialog) {
        AlertDialog(
            onDismissRequest = { showClearDialog = false },
            title = { Text("Очистить список") },
            text = { Text("Удалить все купленные продукты?") },
            confirmButton = {
                TextButton(onClick = { viewModel.clearPurchasedItems(); showClearDialog = false }) {
                    Text("Очистить")
                }
            },
            dismissButton = {
                TextButton(onClick = { showClearDialog = false }) { Text("Отмена") }
            }
        )
    }
    
    if (showAddDialog) {
        AddShoppingItemDialog(
            onDismiss = { showAddDialog = false },
            onAdd = { viewModel.addItem(it); showAddDialog = false }
        )
    }
}

@Composable
fun SpendingAnalyticsCard(
    totalRemaining: Float,
    totalSpentThisWeek: Float,
    spendingByCategory: List<com.yourapp.recipes.data.local.dao.CategorySpending>,
    numberFormat: NumberFormat
) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(modifier = Modifier.padding(16.dp)) {
            Text("📊 Аналитика расходов", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleMedium)
            Spacer(Modifier.height(12.dp))
            
            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceEvenly) {
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    Text("${numberFormat.format(totalRemaining)} ₽", style = MaterialTheme.typography.titleLarge, color = MaterialTheme.colorScheme.error)
                    Text("Осталось", style = MaterialTheme.typography.bodySmall)
                }
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    Text("${numberFormat.format(totalSpentThisWeek)} ₽", style = MaterialTheme.typography.titleLarge, color = MaterialTheme.colorScheme.primary)
                    Text("За неделю", style = MaterialTheme.typography.bodySmall)
                }
            }
            
            if (spendingByCategory.isNotEmpty()) {
                Spacer(Modifier.height(12.dp))
                Text("По категориям:", fontWeight = FontWeight.Bold, style = MaterialTheme.typography.titleSmall)
                spendingByCategory.forEach { cat ->
                    Row(Modifier.fillMaxWidth().padding(vertical = 2.dp), horizontalArrangement = Arrangement.SpaceBetween) {
                        Text("${getCategoryEmoji(cat.category)} ${cat.category}")
                        Text("${numberFormat.format(cat.total)} ₽")
                    }
                }
            }
        }
    }
}

fun getCategoryEmoji(category: String): String = when (category) {
    "vegetables" -> "🥬"
    "meat" -> "🥩"
    "fish" -> "🐟"
    "dairy" -> "🥛"
    "groceries" -> "🌾"
    "spices" -> "🌶️"
    "bakery" -> "🍞"
    "frozen" -> "❄️"
    "drinks" -> "🥤"
    else -> "📦"
}

@Composable
fun CategoryHeader(category: String, itemCount: Int) {
    val categoryIcon = getCategoryEmoji(category)
    
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .background(MaterialTheme.colorScheme.surfaceVariant)
            .padding(horizontal = 16.dp, vertical = 8.dp),
        horizontalArrangement = Arrangement.SpaceBetween,
        verticalAlignment = Alignment.CenterVertically
    ) {
        Text(text = "$categoryIcon $category", style = MaterialTheme.typography.titleSmall)
        Text(text = "$itemCount", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.onSurfaceVariant)
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ShoppingListItem(
    item: ShoppingItem,
    onTogglePurchased: (Boolean) -> Unit,
    onDelete: () -> Unit,
    onPriceChange: (Float) -> Unit,
    modifier: Modifier = Modifier
) {
    var showDeleteConfirm by remember { mutableStateOf(false) }
    var showPriceDialog by remember { mutableStateOf(false) }
    var priceText by remember(item) { mutableStateOf(if (item.price > 0) item.price.toString() else "") }
    
    Card(
        modifier = modifier.fillMaxWidth().padding(vertical = 2.dp),
        colors = CardDefaults.cardColors(
            containerColor = if (item.isPurchased) MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.5f)
            else MaterialTheme.colorScheme.surface
        )
    ) {
        Row(
            modifier = Modifier.fillMaxWidth().padding(12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Checkbox(
                checked = item.isPurchased,
                onCheckedChange = onTogglePurchased,
                colors = CheckboxDefaults.colors(checkedColor = MaterialTheme.colorScheme.primary)
            )
            Spacer(Modifier.width(12.dp))
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = item.name,
                    style = MaterialTheme.typography.bodyLarge,
                    textDecoration = if (item.isPurchased) TextDecoration.LineThrough else TextDecoration.None,
                    color = if (item.isPurchased) MaterialTheme.colorScheme.onSurface.copy(alpha = 0.5f) else MaterialTheme.colorScheme.onSurface
                )
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp), verticalAlignment = Alignment.CenterVertically) {
                    Text("${item.displayQuantity} ${item.unit}", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
                    if (item.price > 0) {
                        Text("× ${item.price} ₽", style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.primary)
                        Text("= ${(item.price * item.quantity).toInt()} ₽", style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.primary)
                    }
                }
            }
            IconButton(onClick = { showPriceDialog = true }) {
                Icon(Icons.Default.AttachMoney, "Цена", tint = if (item.price > 0) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant)
            }
            IconButton(onClick = { showDeleteConfirm = true }) {
                Icon(Icons.Default.Delete, "Удалить", tint = MaterialTheme.colorScheme.error)
            }
        }
    }
    
    if (showDeleteConfirm) {
        AlertDialog(
            onDismissRequest = { showDeleteConfirm = false },
            title = { Text("Удалить продукт") },
            text = { Text("Удалить ${item.name} из списка?") },
            confirmButton = { TextButton(onClick = { onDelete(); showDeleteConfirm = false }) { Text("Удалить") } },
            dismissButton = { TextButton(onClick = { showDeleteConfirm = false }) { Text("Отмена") } }
        )
    }
    
    if (showPriceDialog) {
        AlertDialog(
            onDismissRequest = { showPriceDialog = false },
            title = { Text("Цена за ${item.unit}") },
            text = {
                OutlinedTextField(
                    value = priceText,
                    onValueChange = { priceText = it },
                    label = { Text("Цена (₽)") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true
                )
            },
            confirmButton = { TextButton(onClick = { onPriceChange(priceText.toFloatOrNull() ?: 0f); showPriceDialog = false }) { Text("Сохранить") } },
            dismissButton = { TextButton(onClick = { showPriceDialog = false }) { Text("Отмена") } }
        )
    }
}

@Composable
fun AddShoppingItemDialog(
    onDismiss: () -> Unit,
    onAdd: (ShoppingItem) -> Unit
) {
    var name by remember { mutableStateOf("") }
    var quantity by remember { mutableStateOf("") }
    var unit by remember { mutableStateOf("шт") }
    var category by remember { mutableStateOf("other") }
    var price by remember { mutableStateOf("") }
    
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Добавить продукт") },
        text = {
            Column {
                OutlinedTextField(value = name, onValueChange = { name = it }, label = { Text("Название") }, modifier = Modifier.fillMaxWidth())
                Spacer(Modifier.height(8.dp))
                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    OutlinedTextField(value = quantity, onValueChange = { quantity = it }, label = { Text("Кол-во") }, modifier = Modifier.weight(1f), keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number))
                    OutlinedTextField(value = unit, onValueChange = { unit = it }, label = { Text("Ед.") }, modifier = Modifier.weight(1f))
                }
                Spacer(Modifier.height(8.dp))
                OutlinedTextField(value = price, onValueChange = { price = it }, label = { Text("Цена за ед. (₽)") }, modifier = Modifier.fillMaxWidth(), keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number))
            }
        },
        confirmButton = {
            TextButton(
                onClick = { onAdd(ShoppingItem(name = name, quantity = quantity.toFloatOrNull() ?: 1f, unit = unit, category = category, price = price.toFloatOrNull() ?: 0f)) },
                enabled = name.isNotBlank()
            ) { Text("Добавить") }
        },
        dismissButton = { TextButton(onClick = onDismiss) { Text("Отмена") } }
    )
}
