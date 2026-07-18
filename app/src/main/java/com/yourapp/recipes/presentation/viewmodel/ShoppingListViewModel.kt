package com.yourapp.recipes.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.yourapp.recipes.domain.model.ShoppingItem
import com.yourapp.recipes.domain.repository.ShoppingListRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class ShoppingListViewModel @Inject constructor(
    private val shoppingListRepository: ShoppingListRepository
) : ViewModel() {
    
    val itemsGroupedByCategory = shoppingListRepository.getItemsGroupedByCategory()
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5000), emptyMap())
    
    private val _isExporting = MutableStateFlow(false)
    val isExporting: StateFlow<Boolean> = _isExporting.asStateFlow()
    
    fun toggleItemPurchased(itemId: Long, isPurchased: Boolean) {
        viewModelScope.launch {
            shoppingListRepository.updatePurchasedStatus(itemId, isPurchased)
        }
    }
    
    fun deleteItem(itemId: Long) {
        viewModelScope.launch {
            shoppingListRepository.deleteItem(itemId)
        }
    }
    
    fun clearPurchasedItems() {
        viewModelScope.launch {
            shoppingListRepository.clearPurchasedItems()
        }
    }
    
    fun addItem(item: ShoppingItem) {
        viewModelScope.launch {
            shoppingListRepository.addItem(item)
        }
    }
    
    fun exportToText(): String {
        val items = itemsGroupedByCategory.value
        val sb = StringBuilder()
        sb.appendLine("📝 Список покупок")
        sb.appendLine("=" .repeat(30))
        
        items.forEach { (category, items) ->
            sb.appendLine("\n${getCategoryEmoji(category)} $category:")
            items.filter { !it.isPurchased }.forEach { item ->
                sb.appendLine("  ☐ ${item.displayText}")
            }
        }
        
        return sb.toString()
    }
    
    private fun getCategoryEmoji(category: String): String {
        return when (category) {
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
    }
}
